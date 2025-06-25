using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;

namespace RedGaint.Network.GameSessionModule
{
    public class GameSession
    {
        private readonly LobbyService _lobbyService;
        private readonly ILogger<GameSession> _logger;

        public GameSession(LobbyService lobbyService, ILogger<GameSession> logger)
        {
            _lobbyService = lobbyService;
            _logger = logger;
        }

        [CloudCodeFunction("StartOrJoinSession")]
        public async Task<SessionResponse> StartOrJoinSession(IExecutionContext ctx, SessionRequest request)
        {
            try
            {
                return await _lobbyService.HandleStartOrJoinSession(ctx, request);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to start or join session");
                return new SessionResponse { Message = $"Error: {ex.Message}" };
            }
        }

        [CloudCodeFunction("GetLobbyPlayers")]
        public async Task<List<PlayerSummary>> GetLobbyPlayers(IExecutionContext ctx, LobbyRequest request)
        {
            return await _lobbyService.GetLobbyPlayers(ctx, request.lobbyId);
        }

        [CloudCodeFunction("StartDedicatedServerForLobby")]
        public async Task<string> StartDedicatedServerForLobby(IExecutionContext ctx, string lobbyId)
        {
            return await _lobbyService.StartDedicatedServerForLobby(ctx, lobbyId);
        }
    }
}