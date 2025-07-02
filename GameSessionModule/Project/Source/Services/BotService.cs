using System;
using System.Collections.Generic;
using Unity.Services.Lobby.Model;

namespace RedGaint.Network.GameSessionModule
{
    public class BotService
    {
        public Player CreateBot(int botIndex)
        {
            return new Player(
                id: $"bot_{Guid.NewGuid()}",
                data: new Dictionary<string, PlayerDataObject>
                {
                    { "playerName", new PlayerDataObject($"Bot_{botIndex}", PlayerDataObject.VisibilityEnum.Public) },
                    { "characterId", new PlayerDataObject("bot_char", PlayerDataObject.VisibilityEnum.Public) },
                    { "isBot", new PlayerDataObject("true", PlayerDataObject.VisibilityEnum.Member) }
                });
        }

        public List<Player> CreateMultipleBots(int count)
        {
            var bots = new List<Player>();
            for (int i = 0; i < count; i++)
            {
                bots.Add(CreateBot(i + 1));
            }
            return bots;
        }
    }
}