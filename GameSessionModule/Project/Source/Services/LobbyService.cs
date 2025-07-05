using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RedGaint.Network.Config;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.Lobby.Model;

namespace RedGaint.Network.GameSessionModule
{
    public class LobbyService
    {
        private readonly IGameApiClient _client;
        private readonly IPushClient _pushClient;
        private readonly ILogger<LobbyService> _logger;
        private readonly LobbyMonitorService _monitorService;
        private readonly DedicatedServerService _serverService;
        private readonly PlayerDataBuilder _dataBuilder;
        
        public LobbyService(
            IGameApiClient client,
            IPushClient pushClient,
            ILogger<LobbyService> logger,
            LobbyMonitorService monitorService,
            DedicatedServerService serverService,
            PlayerDataBuilder dataBuilder)
        {
            _client = client;
            _pushClient = pushClient;
            _logger = logger;
            _monitorService = monitorService;
            _serverService = serverService;
            _dataBuilder = dataBuilder;
        }
        
        public async Task<SessionResponse> HandleStartOrJoinSession(IExecutionContext ctx, SessionRequest req)
        {
            CloudDebugLogger.LogInfo("➡️ Entered HandleStartOrJoinSession()");

            string playerId = ctx.PlayerId;
            string token = ctx.AccessToken;
            string xpGroup = _dataBuilder.DetermineXpGroup(req.Xp);

            Dictionary<string, PlayerDataObject> playerData = _dataBuilder.BuildPlayerData(
                playerId: playerId,
                characterId: req.CharacterId,
                playerName: req.PlayerName,
                xp: req.Xp
            );
            
            List<Lobby> lobbies = await QueryAvailableLobbies(ctx: ctx, token: token);
            Lobby? match = FindMatchingLobby(lobbies: lobbies, xpGroup: xpGroup);

            var respose = new SessionResponse();

            return match != null
                ? await JoinExistingLobby(ctx, token, match.Id, playerId, playerData)
                : await CreateNewLobby(ctx, token, playerId, xpGroup, playerData);
        }

        private async Task<SessionResponse> JoinExistingLobby(
            IExecutionContext ctx,
            string token,
            string lobbyId,
            string playerId,
            Dictionary<string, PlayerDataObject> playerData)
        {
            CloudDebugLogger.LogInfo("➡️ Entered JoinExistingLobby()");

            Player player = new Player { Id = playerId, Data = playerData };

            ApiResponse<Lobby> response = await _client.Lobby.JoinLobbyByIdAsync(
                executionContext: ctx,
                accessToken: token,
                lobbyId: lobbyId,
                player: player
            );

            await _monitorService.CheckAndStartServerIfLobbyFull(ctx, response.Data);

            return new SessionResponse
            {
                LobbyId = response.Data.Id,
                LobbyName = response.Data.Name,
                Players = await GetLobbyPlayers(ctx, response.Data.Id), // Add this line
                Message = $"Player {playerId} joined existing lobby."
            };
        }

        private async Task<SessionResponse> CreateNewLobby(
            IExecutionContext ctx,
            string token,
            string playerId,
            string xpGroup,
            Dictionary<string, PlayerDataObject> playerData)
        {
           CloudDebugLogger.LogInfo("➡️ Entered CreateNewLobby()");

            string name = $"AutoLobby_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            Player player = new Player(id: playerId, data: playerData);

            Dictionary<string, DataObject> data = new Dictionary<string, DataObject>
            {
                {
                    "xpGroup",
                    new DataObject(
                        visibility: DataObject.VisibilityEnum.Public,
                        value: xpGroup
                    )
                },
                {
                    "createdAt",
                    new DataObject(
                        visibility: DataObject.VisibilityEnum.Member,
                        value: DateTime.UtcNow.ToString("o")
                    )
                }
            };

            CreateRequest request = new CreateRequest(
                name: name,
                maxPlayers: GameConfig.MaxPlayers,
                isPrivate: false,
                player: player,
                data: data
            );

            ApiResponse<Lobby> result = await _client.Lobby.CreateLobbyAsync(
                executionContext: ctx,
                accessToken: token,
                createRequest: request
            );

            _ = Task.Run(() =>
                _monitorService.MonitorLobbyTimeout(
                    ctx: ctx,
                    lobbyId: result.Data.Id
                )
            );

            return new SessionResponse
            {
                LobbyId = result.Data.Id,
                LobbyName = result.Data.Name,
               Players = await GetLobbyPlayers(ctx, result.Data.Id), // Add this line
                Message = $"Player {playerId} created a new lobby."
            };
        }

        private async Task<List<Lobby>> QueryAvailableLobbies(IExecutionContext ctx, string token)
        {
            CloudDebugLogger.LogInfo("➡️ Entered QueryAvailableLobbies()");

            QueryFilter filter = new QueryFilter(
                field: QueryFilter.FieldEnum.IsLocked,
                op: QueryFilter.OpEnum.EQ,
                value: "false"
            );

            QueryRequest request = new QueryRequest
            {
                Filter = new List<QueryFilter> { filter }
            };

            ApiResponse<QueryResponse> response = await _client.Lobby.QueryLobbiesAsync(
                executionContext: ctx,
                accessToken: token,
                queryRequest: request
            );

            return response.Data?.Results ?? new List<Lobby>();
        }

        private Lobby? FindMatchingLobby(List<Lobby> lobbies, string xpGroup)
        {
            CloudDebugLogger.LogInfo("➡️ Entered FindMatchingLobby()");

            return lobbies.FirstOrDefault(lobby =>
                lobby.Data != null &&
                lobby.Data.TryGetValue("xpGroup", out var group) &&
                group.Value == xpGroup &&
                lobby.Players.Count < GameConfig.MaxPlayers);
        }

        public async Task<List<PlayerSummary>> GetLobbyPlayers(IExecutionContext ctx, string lobbyId)
        {
            CloudDebugLogger.LogInfo("➡️ Entered GetLobbyPlayers()");

            ApiResponse<Lobby> response = await _client.Lobby.GetLobbyAsync(
                executionContext: ctx,
                accessToken: ctx.AccessToken,
                lobbyId: lobbyId
            );

            List<PlayerSummary> players = new List<PlayerSummary>();
            var server = _serverService.GetAllocatedServer(lobbyId);
            bool isLobbyReady = response.Data.Players.Count >= GameConfig.MaxPlayers && server != null;

            int index = 1;
            foreach (Player player in response.Data.Players)
            {
                string displayName = player.Data?.TryGetValue("playerName", out var nameObj) == true
                    ? nameObj.Value
                    : "Unknown";
                string selectedCharacterId = player.Data?.TryGetValue("characterId", out var charObj) == true
                    ? charObj.Value
                    : "None";

                players.Add(new PlayerSummary
                {
                    PlayerId = player.Id,
                    DisplayName = displayName,
                    SelectedCharacterId = selectedCharacterId,
                    IsLobbyReady = isLobbyReady,
                    JoinOrder = index++,
                    MaxPlayersAllowed = GameConfig.MaxPlayers
                });
            }

            return players;
        }
    }
}
