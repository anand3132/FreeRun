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

        public string CurrentSelectedCharacterId;
        public string CurrentSelectedLevelId;

        public int XP;
        public int Coins;
        public int ProgressLevel;

        public Dictionary<string, string> Achievement = new();
        public Dictionary<string, string> Inbox = new();
        public Dictionary<string, string> PlayerSettings= new();
    }
    
   
}