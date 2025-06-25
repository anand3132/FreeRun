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
        private const int MaxPlayers = 4;
        private const int LobbyTimeoutSeconds = 30;

        public LobbyMonitorService(IGameApiClient client, ILogger<LobbyMonitorService> logger)
        {
            _client = client;
            _logger = logger;
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

                    for (int i = 0; i < botsToAdd; i++)
                    {
                        var bot = new Player(
                            id: $"bot_{Guid.NewGuid()}",
                            data: new Dictionary<string, PlayerDataObject>
                            {
                                { "playerName", new PlayerDataObject($"Bot_{i + 1}",PlayerDataObject.VisibilityEnum.Public) },
                                { "characterId", new PlayerDataObject("bot_char",PlayerDataObject.VisibilityEnum.Public ) },
                                { "isBot", new PlayerDataObject("true",PlayerDataObject.VisibilityEnum.Member) }
                            });

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
