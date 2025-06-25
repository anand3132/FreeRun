using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace RedGaint.Network.GameSessionModule
{
    public class DedicatedServerService
    {
        private readonly ILogger<DedicatedServerService> _logger;

        public DedicatedServerService(ILogger<DedicatedServerService> logger)
        {
            _logger = logger;
        }

        public async Task<string> StartDedicatedServerForLobby(IExecutionContext ctx, string lobbyId)
        {
            _logger.LogInformation($"Starting dedicated server for lobby {lobbyId}");
            return await Task.FromResult("game-server-ip-or-session-id");
        }
    }
}