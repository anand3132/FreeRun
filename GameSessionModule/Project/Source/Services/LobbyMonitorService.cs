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
        //Services 
        private readonly IGameApiClient _client;
        private readonly ILogger<LobbyMonitorService> _logger;
        private readonly BotService _botService;
        private readonly DedicatedServerService _serverService;

        public LobbyMonitorService(
            IGameApiClient client,
            ILogger<LobbyMonitorService> logger,
            BotService botService,
            DedicatedServerService serverService)
        {
            _client = client;
            _logger = logger;
            _botService = botService;
            _serverService = serverService;
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
                        await _client.Lobby.JoinLobbyByIdAsync(
                            executionContext: ctx,
                            accessToken: ctx.AccessToken,
                            lobbyId: lobbyId,
                            player: bot
                        );                    }

                    // Fetch updated lobby with bots included
                    ApiResponse<Lobby> response = await _client.Lobby.GetLobbyAsync(ctx, ctx.AccessToken, lobbyId);
                    Lobby lobbyData = response.Data;
                    await CheckAndStartServerIfLobbyFull(ctx, lobbyData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Lobby timeout handling failed for lobby {lobbyId}");
            }
        }
        
        
        // public async Task MonitorLobbyTimeout(IExecutionContext ctx, string lobbyId)
        // {
        //     await Task.Delay(TimeSpan.FromSeconds(LobbyTimeoutSeconds));
        //
        //     try
        //     {
        //         var lobby = await _client.Lobby.GetLobbyAsync(ctx, ctx.AccessToken, lobbyId);
        //
        //         if (lobby.Data.Players.Count < MaxPlayers)
        //         {
        //             int botsToAdd = MaxPlayers - lobby.Data.Players.Count;
        //             var bots = _botService.CreateMultipleBots(botsToAdd);
        //
        //             foreach (var bot in bots)
        //             {
        //                 // await _client.Lobby.AddPlayerAsync(ctx, ctx.AccessToken, lobbyId, bot);
        //             }
        //
        //             // Could start the server here if needed
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, $"Lobby timeout handling failed for lobby {lobbyId}");
        //     }
        // }
    }
}
