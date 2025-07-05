using System;
using Unity.Services.Lobby.Model;

namespace RedGaint.Network.GameSessionModule
{
    public class BotService
    {
        private readonly PlayerDataBuilder _dataBuilder;
        private readonly Random _random = new Random();

        public BotService(PlayerDataBuilder dataBuilder)
        {
            _dataBuilder = dataBuilder;
        }

        public Player CreateBot(string unityPlayerId, int botIndex)
        {
            int characterId = _random.Next(1, 5); // Random value from 1 to 4
            int xp = _random.Next(100, 999);     // XP range from 100 to 1000

            var playerData = _dataBuilder.BuildPlayerData(
                playerId: unityPlayerId,
                characterId: characterId.ToString(),
                playerName: $"Bot_{botIndex}",
                xp: xp
            );

            // Add bot-specific fields if needed
            playerData.Add("isBot", new PlayerDataObject("true", PlayerDataObject.VisibilityEnum.Member));
            playerData.Add("botIndex", new PlayerDataObject(botIndex.ToString(), PlayerDataObject.VisibilityEnum.Member));

            return new Player(id: unityPlayerId, data: playerData);
        }
    }
}