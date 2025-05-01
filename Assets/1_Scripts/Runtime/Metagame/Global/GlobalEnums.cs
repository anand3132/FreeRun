
using UnityEngine;

namespace RedGaint
{
    public static class GlobalEnums
    {
        public enum Mode
        {
            Random,
            Sequence,
            Stack,
            Shuffle,
            RoundRobin,
            ReverseSequence,
            Cluster,
            SingleShot,
            DoubleShot,
        }

        public enum GameTeam
        {
            None = 0,
            TeamRed,
            TeamBlue,
            TeamYellow,
            TeamGreen,
        }

        public enum PowerUpType
        {
            //None,
            Sprint,
            Shield,
            Sludge,
            Bomb
        }
        public enum BotType
        {
            Max,
            Attacker,
            Defender,
            Runner,
            Random,
            Balanced
        }
        public enum LogLevel
        {
            FullLog,
            PartialLog,
            ErrorOnly
        }
        public enum GunType
        {
            Gun1,
            Gun2,
            Gun3,
            Gun4, 
            Gun5,
        }

        public enum CheckPointType
        {
            None,
            WayPoint,
            DefendPoint,
            Destination,
            SpawnPoint
        }
        public enum DifficultyTiers
        {
            Easy,
            Normal,
            Hard
        }
        public enum RotationMode
        {
            RandomMode,
            SineWaveMode
        }
    }


    public static class GlobalStaticVariables
    {
        public static float GameSectionTime { get; private set; }
        public static float BotFollowTimer { get; private set; }

        public static readonly string GameScene = "GameScene01";
        public static readonly string MetaScene = "MetagameScene";
        
        //signin tokens
        //Todo : need to encrypt all
        public static readonly string AppleToken = "NotDefined";
        public static readonly string GoolgeToken = "NotDefined";
        public static readonly string FacebookToken = "NotDefined";
        public static readonly string AnonimusToken = "NotDefined";
        
        //Asset Locations
        public static readonly string TurntablePrefabPath="4_Prefabs/Metagame/Stage/Turntable";
        // public static string UserName = "NotDefined";
        // public static string Password = "NotDefined";
        
        public static bool UserLoggedInStatus = false;
        
        

        // public static void LoadFromScriptableObject(UserProfile data)
        // {
        //     if (data == null)
        //     {
        //       //  BugsBunny.LogRed("GlobalStaticVariables: ScriptableObject data is null. Please assign the GameDataScriptableObject.",);
        //         return;
        //     }
        //
        //     // TeamRedColor = data.teamRedColor;
        //     // GameSectionTime = data.gameSectionTime;
        //     // BotMaxHealth = data.botMaxHealth;
        //     // PlayerMaxHealth = data.playerMaxHealth;
        //     // BotFollowTimer = data.botMaxFollowTime;
        //     // BugsBunny.Log("GlobalStaticVariables: Data loaded successfully.");
        // }
    }


}//RedGaint