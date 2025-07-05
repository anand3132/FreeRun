using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedGaint.Network.Config;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.Lobby.Model;

namespace RedGaint.Network.GameSessionModule
{
   public class LobbyMonitorService
{
    private readonly IGameApiClient _client;
    private readonly BotService _botService;
    private readonly DedicatedServerService _serverService;
    private readonly AuthService _authService; // ✅ Add this

    public LobbyMonitorService(
        IGameApiClient client,
        BotService botService,
        DedicatedServerService serverService,
        AuthService authService // ✅ Inject
    )
    {
        _client = client;
        _botService = botService;
        _serverService = serverService;
        _authService = authService;
    }
    public async Task CheckAndStartServerIfLobbyFull(IExecutionContext ctx, Lobby lobby)
    {
        if (lobby.Players.Count >= GameConfig.MaxPlayers) 
        {
            await _serverService.StartTheServer(
                ctx: ctx,
                lobbyId: lobby.Id,
                players: lobby.Players
            );
        }
    }
    public async Task MonitorLobbyTimeout(IExecutionContext ctx, string lobbyId)
    {
        await Task.Delay(TimeSpan.FromSeconds(GameConfig.LobbyTimeoutSeconds));

        try
        {
            var lobby = await _client.Lobby.GetLobbyAsync(ctx, ctx.AccessToken, lobbyId);

            if (lobby.Data.Players.Count < GameConfig.MaxPlayers)
            {
                int botsToAdd = GameConfig.MaxPlayers - lobby.Data.Players.Count;
                var bots = _botService.CreateMultipleBots(botsToAdd);

                foreach (var bot in bots)
                {
                    var botToken = await _authService.GetAccessTokenAsync(); // ✅ Use same method

                    // You can optionally build a custom ExecutionContext with bot.Id if needed
                    await _client.Lobby.JoinLobbyByIdAsync(
                        executionContext: ctx, // or mock context if needed
                        accessToken: botToken,
                        lobbyId: lobbyId,
                        player: bot
                    );
                }

                var updatedLobby = await _client.Lobby.GetLobbyAsync(ctx, ctx.AccessToken, lobbyId);
                await CheckAndStartServerIfLobbyFull(ctx, updatedLobby.Data);
            }
        }
        catch (Exception ex)
        {
            CloudDebugLogger.LogError(ex, $"❌ Lobby timeout handling failed for lobby {lobbyId}");
        }
    }
}

}
