using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RedGaint.Network.Runtime.UserData
{
    [Serializable]
    public class PlayerProfileData
    {
        public string PlayerId;
        public string AvatarId;
        public string Username;

        public string CharacterId;
        public string CurrentLevelId;

        public int XP;
        public int Coins;
        public int Gems;

        public int ProgressLevel;

        public Dictionary<string, string> Achievement = new();
        public Dictionary<string, string> Inbox = new();
        public Dictionary<string, string> PlayerSettings= new();
        public bool isGuest;
    }

    [Serializable]
    public class PlayerData
    {
        public string PlayerId { get; set; } // Unique ID for the player
        public string DisplayName { get; set; } // Player's name for display (was: PlayerName)
        public string SelectedCharacterId { get; set; } // Character selected by the player (was: CharacterId)

        public bool IsLobbyReady { get; set; } // True if lobby is full and server is allocated
        public int JoinOrder { get; set; } // The player's join position (1-based)
        public int MaxPlayersAllowed { get; set; }
    }
}