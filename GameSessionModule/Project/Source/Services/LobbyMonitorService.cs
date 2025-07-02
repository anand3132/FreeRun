using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.Lobby.Model;

namespace RedGaint.Network.GameSessionModule
{
    public class LobbyMonitorService
    {
        private readonly IGameApiClient _client;
        private readonly ILogger<LobbyMonitorService> _logger;
        private const int MaxPlayers = 2;
        private const int LobbyTimeoutSeconds = 60;
        private readonly BotService _botService;

        public LobbyMonitorService(
            IGameApiClient client,
            ILogger<LobbyMonitorService> logger,
            BotService botService)
        {
            _client = client;
            _logger = logger;
            _botService = botService;
        }

       
        public async Task MonitorLobbyTimeout(IExecutionContext ctx, string lobbyId)
        {
            await Task.Delay(TimeSpan.FromSeconds(LobbyTimeoutSeconds));

            try
            {
                var lobby = await _client.Lobby.GetLobbyAsync(ctx, ctx.AccessToken, lobbyId);

                if (lobby.Data.Players.Count < MaxPlayers)
                {
                    int botsToAdd = MaxPlayers - lobby.Data.Players.Count;
                    var bots = _botService.CreateMultipleBots(botsToAdd);

                    foreach (var bot in bots)
                    {
                        // await _client.Lobby.AddPlayerAsync(ctx, ctx.AccessToken, lobbyId, bot);
                    }

                    // Could start the server here if needed
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lobby timeout handling failed for lobby {lobbyId}");
            }
        }
    }
}
