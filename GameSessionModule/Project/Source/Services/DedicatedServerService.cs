using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
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

    // In-memory map of lobbyId -> allocated server
    private static readonly Dictionary<string, ServerAllocationResult> _allocatedServers =
        new Dictionary<string, ServerAllocationResult>();

    public DedicatedServerService(
        ILogger<DedicatedServerService> logger,
        HttpHelper httpHelper,
        AuthService authService,
        ServerRegistry serverRegistry)
    {
        _logger = logger;
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
            _logger.LogWarning($"⚠️ Server already allocated for Lobby: {lobbyId}");
            return _allocatedServers[lobbyId];
        }

        _logger.LogInformation($"Allocating server for Lobby: {lobbyId} with {players.Count} players");

        string token = await _authService.GetAccessTokenAsync();
        var allocationData = await _serverRegistry.CreateAllocationAsync(token, lobbyId, players);

        if (allocationData != null)
        {
            string allocationId = allocationData.AllocationId;
            MultiplayAllocationInfo serverDetails = await _serverRegistry.GetAllocationDetailsAsync(allocationId, token);

            var serverResult = new ServerAllocationResult
            {
                AllocationId = serverDetails.AllocationId,
                ServerId = serverDetails.ServerId,
                Ipv4 = serverDetails.Ipv4,
                Ipv6 = serverDetails.Ipv6,
                GamePort = serverDetails.GamePort,
                LobbyId = lobbyId
            };

            _allocatedServers[lobbyId] = serverResult;

            _logger.LogInformation($"✅ Server allocated - IP: {serverDetails.Ipv4}, Port: {serverDetails.GamePort}");
            return serverResult;
        }

        _logger.LogError("❌ Failed to allocate server.");
        return null;
    }

    public ServerAllocationResult GetAllocatedServer(string lobbyId)
    {
        return _allocatedServers.TryGetValue(lobbyId, out var server) ? server : null;
    }

    public async Task<bool> ReleaseServerAsync(string lobbyId)
    {
        if (_allocatedServers.TryGetValue(lobbyId, out var server))
        {
            string token = await _authService.GetAccessTokenAsync();
            bool removed = await _serverRegistry.RemoveAllocationAsync(server.AllocationId, token);

            if (removed)
            {
                _allocatedServers.Remove(lobbyId);
                _logger.LogInformation($"🧹 Server allocation removed for Lobby: {lobbyId}");
                return true;
            }
        }

        _logger.LogWarning($"⚠️ No server allocation found for Lobby: {lobbyId} to remove.");
        return false;
    }
}

}//RedGaint.Network.GameSessionModule