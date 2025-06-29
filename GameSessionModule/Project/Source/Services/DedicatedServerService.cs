using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace RedGaint.Network.GameSessionModule
{
    public class DedicatedServerService
    {
        private readonly ILogger<DedicatedServerService> _logger;
        private readonly AuthService _authService;
        private readonly ServerRegistry _serverRegistry;

        public DedicatedServerService(ILogger<DedicatedServerService> logger, AuthService authService, ServerRegistry serverRegistry)
        {
            _logger = logger;
            _authService = authService;
            _serverRegistry = serverRegistry;
        }

        public async Task<ServerAllocationResult> StartTheServer(IExecutionContext ctx, string lobbyId)
        {
            _logger.LogInformation($"Looking for available server for lobby: {lobbyId}");

            var token = await _authService.GetAccessTokenAsync();
            var server = await _serverRegistry.GetAvailableServerAsync();

            if (server == null)
            {
                _logger.LogWarning("No available servers found.");
                throw new System.Exception("No available servers.");
            }

            _logger.LogInformation($"Allocated Server IP: {server.Ip}, Port: {server.Port}");

            return new ServerAllocationResult
            {
                Ip = server.Ip,
                Port = server.Port,
                LobbyId = lobbyId
            };
        }
    }
}