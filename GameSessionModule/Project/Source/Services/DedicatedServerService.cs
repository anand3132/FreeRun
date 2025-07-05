using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;
using Unity.Services.Lobby.Model;

namespace RedGaint.Network.GameSessionModule
{
    public class DedicatedServerService
    {
        private readonly AuthService _authService;
        private readonly ServerRegistry _serverRegistry;
        private readonly HttpHelper _httpHelper;

        private static readonly ConcurrentDictionary<string, ServerAllocationResult> _allocatedServers =
            new ConcurrentDictionary<string, ServerAllocationResult>();

        public DedicatedServerService(
            ILogger<DedicatedServerService> logger, // still required if used elsewhere
            HttpHelper httpHelper,
            AuthService authService,
            ServerRegistry serverRegistry)
        {
            _authService = authService;
            _httpHelper = httpHelper;
            _serverRegistry = serverRegistry;
        }

        public async Task<ServerAllocationResult> StartTheServer(
            IExecutionContext ctx,
            string lobbyId,
            List<Player> players)
        {
            if (_allocatedServers.ContainsKey(lobbyId))
            {
                CloudDebugLogger.LogWarning($"⚠️ Server already allocated for Lobby: {lobbyId}");
                return _allocatedServers[lobbyId];
            }

            CloudDebugLogger.LogInfo($"Allocating server for Lobby: {lobbyId} with {players.Count} players");

            string token = await _authService.GetAccessTokenAsync();
            var allocationData = await _serverRegistry.CreateAllocationAsync(token, lobbyId, players);

            if (allocationData == null)
            {
                CloudDebugLogger.LogError($"❌ Allocation API returned null for Lobby: {lobbyId}");
                return null;
            }

            MultiplayAllocationInfo serverDetails = await _serverRegistry.GetAllocationDetailsAsync(
                allocationData.AllocationId,
                token
            );

            if (serverDetails == null)
            {
                CloudDebugLogger.LogError($"❌ Failed to fetch server details for AllocationId: {allocationData.AllocationId}");
                return null;
            }

            var serverResult = new ServerAllocationResult
            {
                AllocationId = serverDetails.AllocationId,
                ServerId = serverDetails.ServerId,
                Ipv4 = serverDetails.Ipv4,
                Ipv6 = serverDetails.Ipv6,
                GamePort = serverDetails.GamePort,
                LobbyId = lobbyId
            };

            _allocatedServers.TryAdd(lobbyId, serverResult);

            CloudDebugLogger.LogInfo($"✅ Server allocated - IP: {serverDetails.Ipv4}, Port: {serverDetails.GamePort}");
            return serverResult;
        }

        public ServerAllocationResult GetAllocatedServer(string lobbyId)
        {
            return _allocatedServers.TryGetValue(lobbyId, out var server) ? server : null;
        }

        public Task<ServerAllocationResult> TryGetAllocatedServerAsync(string lobbyId)
        {
            _allocatedServers.TryGetValue(lobbyId, out var server);
            return Task.FromResult(server);
        }

        public Task<bool> IsServerAllocatedAsync(string lobbyId)
        {
            return Task.FromResult(_allocatedServers.ContainsKey(lobbyId));
        }

        public async Task<bool> ReleaseServerAsync(string lobbyId)
        {
            if (_allocatedServers.TryGetValue(lobbyId, out var server))
            {
                string token = await _authService.GetAccessTokenAsync();
                bool removed = await _serverRegistry.RemoveAllocationAsync(server.AllocationId, token);

                if (removed)
                {
                    _allocatedServers.TryRemove(lobbyId, out _);
                    CloudDebugLogger.LogInfo($"🧹 Server allocation removed for Lobby: {lobbyId}");
                    return true;
                }

                CloudDebugLogger.LogError($"❌ Failed to remove allocation for Lobby: {lobbyId}");
                return false;
            }

            CloudDebugLogger.LogWarning($"⚠️ No server allocation found for Lobby: {lobbyId} to remove.");
            return false;
        }

        public async Task<string> EndGameSessionAsync(string lobbyId)
        {
            CloudDebugLogger.LogInfo($"Ending game session for lobby {lobbyId}");

            if (!_allocatedServers.ContainsKey(lobbyId))
            {
                CloudDebugLogger.LogInfo($"⚠️ No allocated server found for lobby {lobbyId}");
                return $"⚠️ No allocated server found for lobby {lobbyId}";
            }

            bool success = await ReleaseServerAsync(lobbyId);

            if (success)
            {
                return $"✅ Server deallocated for lobby {lobbyId}";
            }

            return $"⚠️ Server was allocated but failed to deallocate for lobby {lobbyId}";
        }
    }
}
