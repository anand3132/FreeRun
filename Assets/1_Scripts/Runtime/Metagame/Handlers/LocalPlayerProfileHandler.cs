using UnityEngine;
using System.IO;
namespace RedGaint.Network.Runtime
{
    public static class LocalPlayerProfileHandler
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "game_profile.json");

        public static void Save(UserData.PlayerProfileData profileHandler)
        {
            string json = JsonUtility.ToJson(profileHandler, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"User profile saved to: {SavePath}");

        }

        public static UserData.PlayerProfileData  Load()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<UserData.PlayerProfileData >(json);
            }

            return null;
        }

        public static void Delete()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("User profile deleted.");
            }
        }
    }
}