using System.Collections.Generic;
using Unity.Services.Lobby.Model;

namespace RedGaint.Network.GameSessionModule
{
    public class PlayerDataBuilder
    {
        public Dictionary<string, PlayerDataObject> BuildPlayerData(string playerId, string characterId, string playerName, int xp)
        {
            return new Dictionary<string, PlayerDataObject>
            {
                { "playerID", new PlayerDataObject(playerId,PlayerDataObject.VisibilityEnum.Public ) },
                { "playerName", new PlayerDataObject(playerName,PlayerDataObject.VisibilityEnum.Public) },
                { "characterId", new PlayerDataObject(characterId,PlayerDataObject.VisibilityEnum.Public ) },
                { "xp", new PlayerDataObject(xp.ToString(),PlayerDataObject.VisibilityEnum.Member ) }
            };
        }

        public string DetermineXpGroup(int xp)
        {
            if (xp < 1000) return "low";
            if (xp < 2000) return "mid";
            return "high";
        }
    }
}