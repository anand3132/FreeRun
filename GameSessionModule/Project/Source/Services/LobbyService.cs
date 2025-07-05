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

            var playerData = _dataBuilder.BuildPlayerData(
                playerId: playerId,
                characterId: req.CharacterId,
                playerName: req.PlayerName,
                xp: req.Xp
            );

            List<Lobby> lobbies = await QueryAvailableLobbies(ctx, token);
            Lobby? match = FindMatchingLobby(lobbies, xpGroup);

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

            var player = new Player { Id = playerId, Data = playerData };

            var response = await _client.Lobby.JoinLobbyByIdAsync(
                executionContext: ctx,
                accessToken: token,
                lobbyId: lobbyId,
                player: player
            );

            await _monitorService.CheckAndStartServerIfLobbyFull(ctx, response.Data);

            var playerSummaries = MapToPlayerSummaries(response.Data.Players, response.Data.Id);

            return new SessionResponse
            {
                LobbyId = response.Data.Id,
                LobbyName = response.Data.Name,
                Players = playerSummaries,
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
            var player = new Player(id: playerId, data: playerData);

            var data = new Dictionary<string, DataObject>
            {
                { "xpGroup", new DataObject(xpGroup,DataObject.VisibilityEnum.Public ) },
                { "createdAt", new DataObject(DateTime.UtcNow.ToString("o"),DataObject.VisibilityEnum.Member ) }
            };

            var request = new CreateRequest(
                name: name,
                maxPlayers: GameConfig.MaxPlayers,
                isPrivate: false,
                player: player,
                data: data
            );

            var result = await _client.Lobby.CreateLobbyAsync(
                executionContext: ctx,
                accessToken: token,
                createRequest: request
            );

            _ = Task.Run(() => _monitorService.MonitorLobbyTimeout(ctx, result.Data.Id));

            var playerSummaries = MapToPlayerSummaries(result.Data.Players, result.Data.Id);

            return new SessionResponse
            {
                LobbyId = result.Data.Id,
                LobbyName = result.Data.Name,
                Players = playerSummaries,
                Message = $"Player {playerId} created a new lobby."
            };
        }

        private async Task<List<Lobby>> QueryAvailableLobbies(IExecutionContext ctx, string token)
        {
            CloudDebugLogger.LogInfo("➡️ Entered QueryAvailableLobbies()");

            QueryRequest request = new QueryRequest
            {
                Filter = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldEnum.IsLocked, "false",QueryFilter.OpEnum.EQ )
                }
            };

            var response = await _client.Lobby.QueryLobbiesAsync(ctx, token, queryRequest:request);
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

        private List<PlayerSummary> MapToPlayerSummaries(List<Player> players, string lobbyId)
        {
            CloudDebugLogger.LogInfo("➡️ Entered MapToPlayerSummaries()");

            var server = _serverService.GetAllocatedServer(lobbyId);
            bool isLobbyReady = players.Count >= GameConfig.MaxPlayers && server != null;

            var summaries = new List<PlayerSummary>();
            int index = 1;

            foreach (var player in players)
            {
                string displayName = player.Data?.TryGetValue("playerName", out var nameObj) == true ? nameObj.Value : "Unknown";
                string selectedCharacterId = player.Data?.TryGetValue("characterId", out var charObj) == true ? charObj.Value : "None";

                summaries.Add(new PlayerSummary
                {
                    PlayerId = player.Id,
                    DisplayName = displayName,
                    SelectedCharacterId = selectedCharacterId,
                    IsLobbyReady = isLobbyReady,
                    JoinOrder = index++,
                    MaxPlayersAllowed = GameConfig.MaxPlayers
                });
            }

            return summaries;
        }

        public async Task<List<PlayerSummary>> GetLobbyPlayers(IExecutionContext ctx, string lobbyId)
        {
            CloudDebugLogger.LogInfo("➡️ Entered GetLobbyPlayers()");

            var response = await _client.Lobby.GetLobbyAsync(ctx, ctx.AccessToken, lobbyId);
            return MapToPlayerSummaries(response.Data?.Players ?? new List<Player>(), lobbyId);
        }
    }
}
