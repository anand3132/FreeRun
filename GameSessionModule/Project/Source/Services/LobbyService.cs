using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

        private const int MaxPlayers = 4;

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
            string playerId = ctx.PlayerId;
            string token = ctx.AccessToken;
            string xpGroup = _dataBuilder.DetermineXpGroup(req.Xp);

            Dictionary<string, PlayerDataObject> playerData = _dataBuilder.BuildPlayerData(
                playerId: playerId,
                characterId: req.CharacterId,
                playerName: req.PlayerName,
                xp: req.Xp
            );

            List<Lobby> lobbies = await QueryAvailableLobbies(
                ctx: ctx,
                token: token
            );

            Lobby? match = FindMatchingLobby(
                lobbies: lobbies,
                xpGroup: xpGroup
            );

            return match != null
                ? await JoinExistingLobby(ctx, token, match.Id, playerId, playerData)
                : await CreateNewLobby(ctx, token, playerId, xpGroup, playerData);
        }

        private async Task<List<Lobby>> QueryAvailableLobbies(IExecutionContext ctx, string token)
        {
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
            return lobbies.FirstOrDefault(lobby =>
                lobby.Data != null &&
                lobby.Data.TryGetValue("xpGroup", out var group) &&
                group.Value == xpGroup &&
                lobby.Players.Count < MaxPlayers);
        }

        private async Task<SessionResponse> JoinExistingLobby(
            IExecutionContext ctx,
            string token,
            string lobbyId,
            string playerId,
            Dictionary<string, PlayerDataObject> playerData)
        {
            Player player = new Player
            {
                Id = playerId,
                Data = playerData
            };

            ApiResponse<Lobby> response = await _client.Lobby.JoinLobbyByIdAsync(
                executionContext: ctx,
                accessToken: token,
                lobbyId: lobbyId,
                player: player
            );

            if (response.Data.Players.Count == MaxPlayers)
            {
                await _serverService.StartTheServer(
                    ctx: ctx,
                    lobbyId: lobbyId
                );
            }

            return new SessionResponse
            {
                LobbyId = response.Data.Id,
                LobbyName = response.Data.Name,
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
            string name = $"AutoLobby_{DateTime.UtcNow:yyyyMMddHHmmssfff}";

            Player player = new Player(
                id: playerId,
                data: playerData
            );

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
                maxPlayers: MaxPlayers,
                isPrivate: false,
                player: player,
                data: data
            );

            ApiResponse<Lobby>  result = await _client.Lobby.CreateLobbyAsync(
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
                Message = $"Player {playerId} created a new lobby."
            };
        }

        public async Task<List<PlayerSummary>> GetLobbyPlayers(IExecutionContext ctx, string lobbyId)
        {
            ApiResponse<Lobby> response = await _client.Lobby.GetLobbyAsync(
                executionContext: ctx,
                accessToken: ctx.AccessToken,
                lobbyId: lobbyId
            );

            List<PlayerSummary> players = new List<PlayerSummary>();

            foreach (Player player in response.Data.Players)
            {
                string name = player.Data?.TryGetValue("playerName", out var pn) == true ? pn.Value : "Unknown";
                string character = player.Data?.TryGetValue("characterId", out var ch) == true ? ch.Value : "None";

                players.Add(new PlayerSummary
                {
                    PlayerId = player.Id,
                    PlayerName = name,
                    CharacterId = character
                });
            }

            return players;
        }

        public async Task<ServerAllocationResult> StartDedicatedServerForLobby(IExecutionContext ctx, string lobbyId)
        {
            return await _serverService.StartTheServer(ctx, lobbyId);
        }
    }
}
