using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.Lobby.Model;

namespace RedGaint.Network.GameSessionModule
{
    public class TestBotLobbyManager
    {
        private readonly IGameApiClient _client;

        public TestBotLobbyManager(IGameApiClient client)
        {
            _client = client;
        }

        public async Task<string> CreateTestLobbyWithBots(IExecutionContext ctx, int botCount)
        {
            // Step 1: Create a new lobby
            string hostPlayerId = $"host_{Guid.NewGuid()}";
            var lobbyName = $"TestLobby_{Guid.NewGuid().ToString().Substring(0, 5)}";
            
            var createRequest = new CreateRequest(
                name: lobbyName,
                maxPlayers: botCount + 1,
                isPrivate: false,
                player: new Player(
                    id: hostPlayerId,
                    data: new Dictionary<string, PlayerDataObject>
                    {
                        { "playerName", new PlayerDataObject("Host", PlayerDataObject.VisibilityEnum.Member ) }
                    }
                )
            );
            
            var lobbyResponse = await _client.Lobby.CreateLobbyAsync(executionContext:ctx,accessToken: ctx.AccessToken, createRequest:createRequest);
            Lobby lobby = lobbyResponse.Data;
            
            CloudDebugLogger.LogInfo($"✅ Created test lobby '{lobby.Id}' with host '{hostPlayerId}'.");
            
            // Step 2: Add bots to the lobby
            for (int i = 0; i < botCount; i++)
            {
                string botId = $"bot_{Guid.NewGuid()}";
                string botName = $"Bot {i + 1}";
            
                var botPlayer = new Player(
                    id: botId,
                    data: new Dictionary<string, PlayerDataObject>
                    {
                        { "isBot", new PlayerDataObject("true",PlayerDataObject.VisibilityEnum.Member ) },
                        { "playerName", new PlayerDataObject( botName,PlayerDataObject.VisibilityEnum.Member) }
                    }
                );
            
                var joinResponse = await _client.Lobby.JoinLobbyByIdAsync(
                    executionContext: ctx,
                    accessToken: ctx.AccessToken,
                    lobbyId: lobby.Id,
                    player: botPlayer
                );
            
                CloudDebugLogger.LogInfo($"🤖 Bot '{botName}' joined lobby '{lobby.Id}'.");
                lobby = joinResponse.Data;
            }
            
            CloudDebugLogger.LogInfo($"🎉 All bots successfully joined lobby '{lobby.Id}'.");
            return lobby.Id;
        }
    }
}
