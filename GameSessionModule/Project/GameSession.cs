using System;
using Unity.Services.Lobby.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;

namespace RedGaint.Network.GameSessionModule
{
    public class GameSession
    {
        private readonly IGameApiClient _gameApiClient;
        private readonly IPushClient _pushClient;
        private readonly ILogger<GameSession> _logger;
        private readonly Random _random;
        private const int MaxPlayers = 4;
        private const int LobbyTimeoutSeconds = 30; 

        public GameSession(IGameApiClient gameApiClient, IPushClient pushClient, ILogger<GameSession> logger,
            Random random)
        {
            _gameApiClient = gameApiClient;
            _pushClient = pushClient;
            _logger = logger;
            _random = random;
        }
        
        [CloudCodeFunction("SayHello")]
        public string Hello(string name)
        {
            return $"Hello, {name}!";
        }

        public string testLobbyID;
        
        [CloudCodeFunction("StartOrJoinSession")]
        public async Task<SessionResponse> StartOrJoinSession(IExecutionContext executionContext,
            SessionRequest request)
        {
            string playerId = executionContext.PlayerId;
            string accessToken = executionContext.AccessToken;
            string characterId = request.CharacterId;
            string playerName = request.PlayerName;
            string xpGroup = DetermineXpGroup(request.Xp);

            try
            {
                Dictionary<string, PlayerDataObject> playerData = BuildPlayerData(
                    playerId: playerId,
                    characterId: characterId,
                    playerName: playerName,
                    xp: request.Xp
                );

                List<Lobby> availableLobbies = await QueryAvailableLobbies(
                    executionContext: executionContext,
                    accessToken: accessToken
                );

                Lobby matchingLobby = FindMatchingLobby(
                    lobbies: availableLobbies,
                    xpGroup: xpGroup
                );
                if (matchingLobby != null)
                {
                    return await JoinExistingLobby(
                        executionContext: executionContext,
                        accessToken: accessToken,
                        lobbyId: matchingLobby.Id,
                        playerId: playerId,
                        playerData: playerData
                    );
                }

                return await CreateNewLobby(
                    executionContext: executionContext,
                    accessToken: accessToken,
                    playerId: playerId,
                    xpGroup: xpGroup,
                    playerData: playerData
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lobby start/join failed");
                return new SessionResponse
                {
                    LobbyId = testLobbyID,
                    Message = $"Error: {ex.Message}"
                };
            }
        }//StartOrJoinSession
        
        private Dictionary<string, PlayerDataObject> BuildPlayerData(string playerId, string characterId,
            string playerName, int xp)
        {
            return new Dictionary<string, PlayerDataObject>
            {
                {
                    "playerID",
                    new PlayerDataObject(visibility: PlayerDataObject.VisibilityEnum.Public, value: playerId)
                },
                {
                    "playerName",
                    new PlayerDataObject(visibility: PlayerDataObject.VisibilityEnum.Public, value: playerName)
                },
                {
                    "characterId",
                    new PlayerDataObject(visibility: PlayerDataObject.VisibilityEnum.Public, value: characterId)
                },
                { "xp", new PlayerDataObject(visibility: PlayerDataObject.VisibilityEnum.Member, value: xp.ToString()) }
            };
        }

        private async Task<List<Lobby>> QueryAvailableLobbies(IExecutionContext executionContext, string accessToken)
        {
            QueryRequest queryRequest = new QueryRequest
            {
                Filter = new List<QueryFilter>
                {
                    new QueryFilter(field: QueryFilter.FieldEnum.IsLocked, value: "false", op: QueryFilter.OpEnum.EQ)
                }
            };

            ApiResponse<QueryResponse> queryResponse = await _gameApiClient.Lobby.QueryLobbiesAsync(
                executionContext: executionContext,
                accessToken: accessToken,
                queryRequest: queryRequest
            );

            return queryResponse.Data?.Results ?? new List<Lobby>();
        }

        private Lobby FindMatchingLobby(List<Lobby> lobbies, string xpGroup)
        {
            return lobbies.FirstOrDefault(lobby =>
                lobby.Data != null &&
                lobby.Data.TryGetValue("xpGroup", out var group) &&
                group.Value == xpGroup &&
                lobby.Players.Count < MaxPlayers);
        }

        private async Task<SessionResponse> JoinExistingLobby(IExecutionContext executionContext, string accessToken,
            string lobbyId, string playerId, Dictionary<string, PlayerDataObject> playerData)
        {
            ApiResponse<Lobby> joinedLobby = await _gameApiClient.Lobby.JoinLobbyByIdAsync(
                executionContext: executionContext,
                accessToken: accessToken,
                lobbyId: lobbyId,
                player: new Player { Id = playerId, Data = playerData }
            );

            if (joinedLobby.Data.Players.Count == MaxPlayers)
            {
                await StartDedicatedServerForLobby(
                    ctx: executionContext,
                    lobbyId: joinedLobby.Data.Id
                );
            }

            return new SessionResponse
            {
                LobbyId = joinedLobby.Data.Id,
                LobbyName = joinedLobby.Data.Name,
                Message = $"Player {playerId} joined existing lobby."
            };
        }

        private async Task<SessionResponse> CreateNewLobby(IExecutionContext executionContext, string accessToken,
            string playerId, string xpGroup, Dictionary<string, PlayerDataObject> playerData)
        {
            string lobbyName = "AutoLobby_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

            Player player = new Player(id: playerId, data: playerData);

            CreateRequest createRequest = new CreateRequest(
                name: lobbyName,
                maxPlayers: MaxPlayers,
                isPrivate: false,
                player: player,
                data: new Dictionary<string, DataObject>
                {
                    { "xpGroup", new DataObject(visibility: DataObject.VisibilityEnum.Public, value: xpGroup) },
                    {
                        "createdAt",
                        new DataObject(visibility: DataObject.VisibilityEnum.Member,
                            value: DateTime.UtcNow.ToString("o"))
                    }
                }
            );

            ApiResponse<Lobby> createdLobby = await _gameApiClient.Lobby.CreateLobbyAsync(
                executionContext: executionContext,
                accessToken: accessToken,
                createRequest: createRequest
            );

            _ = Task.Run(async () =>
                await MonitorLobbyTimeout(
                    context: executionContext,
                    lobbyId: createdLobby.Data.Id
                )
            );

            return new SessionResponse
            {
                LobbyId = createdLobby.Data.Id,
                LobbyName = createdLobby.Data.Name,
                Message = $"Player {playerId} created a new lobby."
            };
        }
        
        private async Task MonitorLobbyTimeout(IExecutionContext context, string lobbyId)
        {
            await Task.Delay(TimeSpan.FromSeconds(LobbyTimeoutSeconds));

            try
            {
                string? accessToken = context.AccessToken;
                ApiResponse<Lobby> lobby = await _gameApiClient.Lobby.GetLobbyAsync(context, accessToken, lobbyId);

                if (lobby.Data.Players.Count < MaxPlayers)
                {
                    // Add bots
                    int botsToAdd = MaxPlayers - lobby.Data.Players.Count;
                    for (int i = 0; i < botsToAdd; i++)
                    {
                        string botId = "bot_" + Guid.NewGuid();
                        var botPlayer = new Player(
                            id: botId,
                            data: new Dictionary<string, PlayerDataObject>
                            {
                                {
                                    "playerName",
                                    new PlayerDataObject(visibility: PlayerDataObject.VisibilityEnum.Public,
                                        value: $"Bot_{i + 1}")
                                },
                                {
                                    "characterId",
                                    new PlayerDataObject(visibility: PlayerDataObject.VisibilityEnum.Public,
                                        value: "bot_char")
                                },
                                {
                                    "isBot",
                                    new PlayerDataObject(visibility: PlayerDataObject.VisibilityEnum.Member,
                                        value: "true")
                                }
                            });

                        // await _gameApiClient.Lobby.AddPlayerAsync(context, accessToken, lobbyId, botPlayer);
                    }

                    await StartDedicatedServerForLobby(context, lobbyId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to monitor lobby timeout for {lobbyId}");
            }
        }

        private string DetermineXpGroup(int xp)
        {
            if (xp < 1000) return "low";
            else if (xp < 2000) return "mid";
            else return "high";
        }

        [CloudCodeFunction("StartDedicatedServerForLobby")]
        public async Task<string> StartDedicatedServerForLobby(IExecutionContext ctx, string lobbyId)
        {
            _logger.LogInformation($"Triggering dedicated server for lobby {lobbyId}");
            return "game-server-ip-or-session-id";
        }
        
        
        public class LobbyRequest
        {
            public string lobbyId { get; set; }
        }

        [CloudCodeFunction("GetLobbyPlayers")]
        public async Task<List<PlayerSummary>> GetLobbyPlayers(IExecutionContext context, LobbyRequest request)
        {
            try
            {
                string accessToken = context.AccessToken;
                var lobbyId = request.lobbyId;

                ApiResponse<Lobby> lobby = await _gameApiClient.Lobby.GetLobbyAsync(
                    executionContext: context,
                    accessToken: accessToken,
                    lobbyId: lobbyId
                );

                List<PlayerSummary> players = new();
                foreach (var p in lobby.Data.Players)
                {
                    string name = p.Data?.ContainsKey("playerName") == true ? p.Data["playerName"].Value : "Unknown";
                    string character = p.Data?.ContainsKey("characterId") == true ? p.Data["characterId"].Value : "None";

                    players.Add(new PlayerSummary { PlayerId = p.Id, PlayerName = name, CharacterId = character });
                }

                return players;
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to get lobby players: {e.Message}");
                throw;
            }
        }


        public class PlayerSummary
        {
            public string PlayerId { get; set; }
            public string PlayerName { get; set; }
            public string CharacterId { get; set; }
        }

        public class SessionRequest
        {
            public string PlayerId { get; set; }
            public string CharacterId { get; set; }
            public string PlayerName { get; set; }
            public int Xp { get; set; }
        }

        public class SessionResponse
        {
            public string LobbyId { get; set; }
            public string LobbyName { get; set; }
            public string Message { get; set; }
        }
        
    }

    public class PlayerSummary
    {
    }
    //GameSession
}//RedGaint.Network.GameSessionModule
