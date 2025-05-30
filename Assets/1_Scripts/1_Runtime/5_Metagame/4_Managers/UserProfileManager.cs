#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedGaint.Network.Runtime.UserData;
using RedGaint.Utility;
using UnityEngine;

namespace RedGaint.Network.Runtime
{
    public  class UserProfileManager:Singleton<UserProfileManager>, IBugsBunny
    {
        // Create a default empty profile immediately
        public static PlayerProfileData CurrentUser { get; private set; } = new PlayerProfileData();


        public  PlayerProfileData CreateGuestProfile()
        {
            CurrentUser.Username = $"Guest_{SystemInfo.deviceUniqueIdentifier[..6]}";
            CurrentUser.PlayerId = CurrentUser.Username;
            CurrentUser.XP = 0;
            CurrentUser.Coins = 0;
            CurrentUser.ProgressLevel = 1;
            CurrentUser.AvatarId = string.Empty;
            CurrentUser.CharacterId = string.Empty;
            CurrentUser.CurrentLevelId = string.Empty;
            return CurrentUser;
        }

        public  async Task<bool> LoadAsync(bool useCloud, CancellationToken cancellationToken = default)
        {
            useCloud = false;
            if (useCloud)
            {
                var cloudProfile = await CloudPlayerProfileHandler.LoadAsync(cancellationToken);
                if (cloudProfile != null)
                {
                    CurrentUser = cloudProfile;
                    Debug.Log("Loaded profile from cloud.");
                    return true;
                }
                Debug.Log("No cloud profile found, falling back to guest.");
                return false;
            }

            var local = LocalPlayerProfileHandler.Load();
            if (local != null)
            {
                CurrentUser = local;
                Debug.Log("Loaded profile from local.");
                return true;
            }
            else
            {
                CreateGuestProfile();
                Debug.Log("Created guest profile.");
                return false;
            }
        }

        public  async Task SaveAsync(bool toCloud, PlayerProfileData newuser, CancellationToken cancellationToken = default)
        {
            toCloud = false;
            CurrentUser = newuser;
            await SaveAsync(false, cancellationToken);
        }

        private  async Task<bool> SaveAsync(bool toCloud, CancellationToken cancellationToken = default)
        {
            
            if (CurrentUser == null)
            {
                Debug.LogWarning("No current profile to save.");
                return false;
            }

            if (toCloud)
            {
                await CloudPlayerProfileHandler.SaveAsync(CurrentUser, cancellationToken);
                Debug.Log("Saved profile to cloud.");
                return true;
            }
            else
            {
                LocalPlayerProfileHandler.Save(CurrentUser);
                Debug.Log("Saved profile locally.");
                return true;
            }
        }

        public async Task<bool> UpdatePlayerProfile(PlayerProfileData playerProfileData,bool saveOnCloud)
        {
            saveOnCloud = false;
            CurrentUser = playerProfileData;
            return await SaveAsync(saveOnCloud);
        }

        public  void Clear()
        {
            CurrentUser = new PlayerProfileData();
        }

        public bool LogThisClass { get; } = false;
    }
}
// using System.Threading.Tasks;
//
// namespace RedGaint.Network.Runtime
// {
//     public static class GameProfileService
//     {
//         public static UserData.PlayerProfileData CurrentProfileData { get; private set; }
//
//         public static void LoadFromLocal()
//         {
//             CurrentProfileData = UserData.LocalPlayerProfileHandler.Load() ?? new UserData.PlayerProfileData();
//         }
//
//         public static async Task LoadFromCloudAsync()
//         {
//             CurrentProfileData = await UserData.CloudPlayerProfileHandler.LoadAsync() ?? new UserData.PlayerProfileData();
//         }
//
//         public static void SaveLocal() => UserData.LocalPlayerProfileHandler.Save(CurrentProfileData);
//
//         public static async Task SaveCloudAsync() => await UserData.CloudPlayerProfileHandler.SaveAsync(CurrentProfileData);
//     }
//
// }