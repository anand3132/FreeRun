using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace RedGaint.Network.Runtime
{
    public static class LocalPlayerProfileHandler
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "game_profiles.json");

        [System.Serializable]
        private class ProfileListWrapper
        {
            public List<UserData.PlayerProfileData> profiles = new List<UserData.PlayerProfileData>();
        }

        /// <summary>
        /// Saves or updates a profile based on the username.
        /// </summary>
        public static void Save(UserData.PlayerProfileData newProfile)
        {
            var wrapper = LoadAllInternal();

            int index = wrapper.profiles.FindIndex(p => p.Username == newProfile.Username);
            if (index >= 0)
            {
                wrapper.profiles[index] = newProfile; // Update existing
                Debug.Log($"[ProfileHandler] Updated existing profile for user: {newProfile.Username}");
            }
            else
            {
                wrapper.profiles.Add(newProfile); // Append new
                Debug.Log($"[ProfileHandler] Added new profile for user: {newProfile.Username}");
            }

            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[ProfileHandler] Profile data saved to: {SavePath}");
        }

        /// <summary>
        /// Loads a specific user profile by username.
        /// </summary>
        public static UserData.PlayerProfileData Load(string username)
        {
            var wrapper = LoadAllInternal();
            var profile = wrapper.profiles.FirstOrDefault(p => p.Username == username);
            Debug.Log("Loading User Profile From : "+SavePath);
            if (profile != null)
            {
                Debug.Log($"[ProfileHandler] Loaded profile for user: {username}");
            }
            else
            {
                Debug.LogWarning($"[ProfileHandler] No profile found for user: {username}");
            }

            return profile;
        }

        /// <summary>
        /// Loads all stored profiles.
        /// </summary>
        public static List<UserData.PlayerProfileData> LoadAll()
        {
            return LoadAllInternal().profiles;
        }

        /// <summary>
        /// Deletes the entire profile file.
        /// </summary>
        public static void DeleteAll()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[ProfileHandler] All profiles deleted.");
            }
        }

        private static ProfileListWrapper LoadAllInternal()
        {
            if (!File.Exists(SavePath))
                return new ProfileListWrapper();

            try
            {
                string json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<ProfileListWrapper>(json) ?? new ProfileListWrapper();
            }
            catch
            {
                Debug.LogError("[ProfileHandler] Failed to read or parse profile file.");
                return new ProfileListWrapper();
            }
        }
    }
}
