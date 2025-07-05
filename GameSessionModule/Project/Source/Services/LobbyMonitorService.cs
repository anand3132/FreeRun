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
        private readonly AuthService _authService;

        public LobbyMonitorService(
            IGameApiClient client,
            BotService botService,
            DedicatedServerService serverService,
            AuthService authService)
        {
            _client = client;
            _botService = botService;
            _serverService = serverService;
            _authService = authService;
        }

        public async Task CheckAndStartServerIfLobbyFull(IExecutionContext ctx, Lobby lobby)
        {
            CloudDebugLogger.LogInfo($"✅ Checking if lobby '{lobby.Id}' is full...");

            if (lobby.Players.Count >= GameConfig.MaxPlayers)
            {
                CloudDebugLogger.LogInfo($"🚀 Lobby is full. Starting server for lobby '{lobby.Id}'...");
                await _serverService.StartTheServer(
                    ctx: ctx,
                    lobbyId: lobby.Id,
                    players: lobby.Players
                );
                CloudDebugLogger.LogInfo($"✅ Server started for lobby '{lobby.Id}'.");
            }
            else
            {
                CloudDebugLogger.LogInfo(
                    $"ℹ️ Lobby '{lobby.Id}' is not full yet. ({lobby.Players.Count}/{GameConfig.MaxPlayers})");
            }
        }

     public async Task MonitorLobbyTimeout(IExecutionContext ctx, string lobbyId)
{
    CloudDebugLogger.LogInfo($"⏳ Monitoring lobby timeout for lobby '{lobbyId}'...");

    await Task.Delay(TimeSpan.FromSeconds(GameConfig.LobbyTimeoutSeconds));

    try
    {
        CloudDebugLogger.LogInfo($"🔍 Fetching lobby '{lobbyId}' after timeout...");
        var lobbyResponse = await _client.Lobby.GetLobbyAsync(ctx, ctx.AccessToken, lobbyId);
        var lobby = lobbyResponse.Data;

        int existingPlayers = lobby.Players.Count;
        if (existingPlayers < GameConfig.MaxPlayers)
        {
            int botsToAdd = GameConfig.MaxPlayers - existingPlayers;
            CloudDebugLogger.LogInfo(
                $"⚠️ Lobby '{lobbyId}' has only {existingPlayers} players. Adding {botsToAdd} bot(s)...");

            for (int counter = 0; counter < botsToAdd; counter++)
            {
                // 1. Step: Login anonymously → Get idToken
                string idToken = await _authService.LoginAnonymousAsync();

                // 2. Step: Exchange idToken for accessToken
                string accessToken = await _authService.ExchangeIdTokenForAccessToken(idToken);

                // 3. Step: Extract Unity player ID (from access token)
                string unityPlayerId = await _authService.ExtractPlayerIdFromToken(accessToken);

                // 4. Step: Build bot player object
                var botPlayer = _botService.CreateBot(unityPlayerId, counter + 1);

                CloudDebugLogger.LogInfo($"🤖 Adding bot '{botPlayer.Id}' to lobby '{lobbyId}'...");

                // 5. Step: Join the lobby using the bot's access token
                await _client.Lobby.JoinLobbyByIdAsync(
                    executionContext: ctx,
                    accessToken: accessToken,
                    lobbyId: lobbyId,
                    player: botPlayer
                );

                CloudDebugLogger.LogInfo($"✅ Bot '{botPlayer.Id}' joined lobby '{lobbyId}'.");
            }

            // 🔁 Re-fetch and check if game can start
            var updatedLobby = await _client.Lobby.GetLobbyAsync(ctx, ctx.AccessToken, lobbyId);
            CloudDebugLogger.LogInfo($"🔄 Re-checking lobby '{lobbyId}' after adding bots...");
            await CheckAndStartServerIfLobbyFull(ctx, updatedLobby.Data);
        }
        else
        {
            CloudDebugLogger.LogInfo($"✅ Lobby '{lobbyId}' already has enough players. No bots needed.");
        }
    }
    catch (Exception ex)
    {
        CloudDebugLogger.LogError(ex, $"❌ Lobby timeout handling failed for lobby '{lobbyId}'.");
    }
}


    }
}
