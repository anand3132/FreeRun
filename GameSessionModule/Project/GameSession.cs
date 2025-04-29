
using System;
using Unity.Services.Lobby.Model;
using System.Collections.Generic;
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
        
        [CloudCodeFunction("SayHello")]
        public string Hello(string name)
        {
            return $"Hello, {name}!";
        }
        
        public GameSession(IGameApiClient gameApiClient, IPushClient pushClient, ILogger<GameSession> logger, Random random)
        {
            _gameApiClient = gameApiClient;
            _pushClient = pushClient;
            _logger = logger;
            _random = random;
        }
        
        [CloudCodeFunction("StartOrJoinSession")]
        public async Task<SessionResponse> StartOrJoinSession( IExecutionContext context, SessionRequest request)
        {
           
            string? playerId = context.PlayerId;
            string accessToken = context.AccessToken; 
            string characterId = request.CharacterId;
            string playerName = request.PlayerName;
            
            int maxPlayers = 4;
            
            try
            {
                // Prepare QueryRequest for filtering lobbies
                var queryRequest = new QueryRequest
                {
                    Filter = new List<QueryFilter>  
                    {
                        new QueryFilter(
                            QueryFilter.FieldEnum.IsLocked,        
                            "false",                              
                            QueryFilter.OpEnum.EQ             
                        )
                    }
                };

                // Call QueryLobbiesAsync
                ApiResponse<QueryResponse> queryResponse = await _gameApiClient.Lobby.QueryLobbiesAsync(
                     context
                    ,accessToken: accessToken
                    ,queryRequest: queryRequest
                );

                Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>
                {
                    { "playerID", new PlayerDataObject(visibility: PlayerDataObject.VisibilityEnum.Public, value: playerId) },
                    { "playerName", new PlayerDataObject(visibility: PlayerDataObject.VisibilityEnum.Public, value: playerName) },
                    { "characterId", new PlayerDataObject(visibility: PlayerDataObject.VisibilityEnum.Public, value: characterId) }
                };
                // If there are lobbies that match the query
                if (queryResponse.Data?.Results != null && queryResponse.Data.Results.Count > 0)
                {
                    var lobbyId = queryResponse.Data.Results[0].Id;

                    // Join the existing lobby
                    ApiResponse<Lobby> joinedLobby = await _gameApiClient.Lobby.JoinLobbyByIdAsync(
                        context,
                        accessToken:accessToken,
                        lobbyId:lobbyId,
                        player:  new Player { Id = playerId, Data = playerData }
                    );

                    return new SessionResponse
                    {
                        LobbyId = joinedLobby.Data.Id,
                        Message = $"Player {playerId} joined existing lobby."
                    };
                }
                else
                {
                    Player player = new Player(
                        id: playerId,  
                        profile: null,  
                        connectionInfo: null,  
                        data: playerData,  
                        allocationId: null,  
                        joined: DateTime.Now,  
                        lastUpdated: DateTime.Now 
                    );
                    string randomLobbyName = "AutoLobby_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

                    // No open lobbies, create a new one
                    ApiResponse<Lobby> createdLobby = await _gameApiClient.Lobby.CreateLobbyAsync(
                        context,
                        accessToken,
                        createRequest: new CreateRequest(name:randomLobbyName, maxPlayers: maxPlayers, isPrivate: false,
                            player: player));
                    
                    return new SessionResponse
                    {
                        LobbyId = createdLobby.Data.Id,
                        LobbyName = createdLobby.Data.Name,
                        Message = $"Player {playerId} created a new lobby."
                    };
                }
            }
            catch (Exception ex)
            {
                return new SessionResponse
                {
                    LobbyId = null,
                    LobbyName = null,
                    Message = $"Error : Name :  : during lobby creation/join: {ex.Message}"
                };
            }
        }

        public class SessionRequest
        {
            
            public string PlayerId { get; set; }
            public string CharacterId { get; set; }
            public string PlayerName { get; set; }
        }

        public class SessionResponse
        {
            public string LobbyId { get; set; }
            public string LobbyName { get; set; }
            public string Message { get; set; }
        }
    }
}
