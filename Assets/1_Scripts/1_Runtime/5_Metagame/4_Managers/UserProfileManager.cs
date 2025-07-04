#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using RedGaint.Network.Runtime.UserData;
using RedGaint.Utility;

namespace RedGaint.Network.Runtime
{
    public class UserProfileManager : Singleton<UserProfileManager>, IBugsBunny
    {
        public static PlayerProfileData CurrentUser { get; private set; } 

        public PlayerProfileData CreateNewUserProfile(string username,string newPlayerId,bool isGuest=false)
        {
            var profile= new PlayerProfileData
            {
                Username = username,
                PlayerId = newPlayerId,
                AvatarId = Guid.NewGuid().ToString(),
                CharacterId = "1",
                CurrentLevelId = "Level_01",
                XP = 100,
                Coins = 100,
                ProgressLevel = 0,
                isGuest = isGuest
            };
            CurrentUser=profile;
            return CurrentUser;
        }

        public async Task<bool> LoadAsync(bool useCloud, string userName, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            useCloud = false; // force local for editor testing
#endif
            if (useCloud)
            {
                var cloudProfile = await CloudPlayerProfileHandler.LoadAsync(cancellationToken);
                if (cloudProfile != null)
                {
                    CurrentUser = cloudProfile;
                    Debug.Log("✅ Loaded profile from cloud.");
                    return true;
                }

                Debug.LogWarning("⚠ No cloud profile found, falling back to guest.");
                return false;
            }

            var local = LocalPlayerProfileHandler.Load(userName);
            if (local != null)
            {
                CurrentUser = local;
                Debug.Log($"✅ Loaded profile from local for user: {userName}");
                return true;
            }
            Debug.Log("<color=red>Failed to Load user Profile -------------</color>");
            return false;
        }

        public async Task SaveAsync(bool toCloud, PlayerProfileData profileData, CancellationToken cancellationToken = default)
        {
            CurrentUser = profileData;
            await SaveAsync(toCloud, cancellationToken);
        }

        private async Task<bool> SaveAsync(bool toCloud, CancellationToken cancellationToken = default)
        {
            if (CurrentUser == null)
            {
                Debug.LogWarning("⚠ No current profile to save.");
                return false;
            }
            #if UNITY_EDITOR
                toCloud = false;
            #endif
            
            if (toCloud)
            {
                await CloudPlayerProfileHandler.SaveAsync(CurrentUser, cancellationToken);
                Debug.Log("✅ Saved profile to cloud.");
            }
            else
            {
                LocalPlayerProfileHandler.Save(CurrentUser);
                Debug.Log("✅ Saved profile locally.");
            }

            return true;
        }

        public async Task<bool> UpdatePlayerProfile( bool saveOnCloud)
        {
            // if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.Username))
            // {
            //     Debug.Log("⚠ No valid profile loaded. Creating new profile for :  "+newPlayerId);
            //   //  CurrentUser = CreateNewUserProfile(newPlayerId);
            // }

            return await SaveAsync(saveOnCloud);
        }

        public void Clear()
        {
            CurrentUser = new PlayerProfileData();
        }

        public bool LogThisClass { get; } = false;
    }
}
