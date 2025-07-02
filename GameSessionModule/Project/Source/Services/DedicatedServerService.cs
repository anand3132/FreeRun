using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Core;
using Unity.Services.Lobby.Model;

namespace RedGaint.Network.GameSessionModule
{
    public class DedicatedServerService
    {
        private readonly ILogger<DedicatedServerService> _logger;
        private readonly AuthService _authService;
        private readonly ServerRegistry _serverRegistry;
        private readonly HttpHelper _httpHelper;

        public DedicatedServerService(ILogger<DedicatedServerService> logger, HttpHelper httpHelper,AuthService authService, ServerRegistry serverRegistry)
        {
            _logger = logger;
            _authService = authService;
            _httpHelper = httpHelper;
            _serverRegistry = serverRegistry;
        }

        public async Task<ServerAllocationResult> StartTheServer(IExecutionContext ctx, string lobbyId, List<Player> players)
        {
            _logger.LogInformation($"Allocating server for Lobby: {lobbyId} with {players.Count} players");

            string token = await _authService.GetAccessTokenAsync();
            var allocationData = await _serverRegistry.CreateAllocationAsync(token, lobbyId, players);

            if (allocationData != null)
            {
                string allocationId = allocationData.AllocationId;
                MultiplayAllocationInfo serverDetails = await _serverRegistry.GetAllocationDetailsAsync(allocationId, token);
    
                _logger.LogInformation($"✅ Server allocated - IP: {serverDetails.Ipv4}, Port: {serverDetails.GamePort}");

                return new ServerAllocationResult
                {
                    AllocationId = serverDetails.AllocationId,
                    ServerId = serverDetails.ServerId,
                    Ipv4 = serverDetails.Ipv4,
                    Ipv6 = serverDetails.Ipv6,
                    GamePort = serverDetails.GamePort,
                    LobbyId = lobbyId
                };
            }

            _logger.LogError("❌ Failed to allocate server.");
            return null;
        }

    }//DedicatedServerService
}//RedGaint.Network.GameSessionModule