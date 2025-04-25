#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using RedGaint.Network.Runtime.UserData;
using UnityEngine;

namespace RedGaint.Network.Runtime
{
    public static class GameProfileManager
    {
        public static PlayerProfileData? Current { get; private set; }

        public static bool IsLoggedIn => Current != null && !Current.Username.StartsWith("Guest", StringComparison.OrdinalIgnoreCase);

        public static string DisplayName => Current?.Username ?? "Guest";

        public static void CreateGuestProfile()
        {
            string guestId = $"Guest-{SystemInfo.deviceUniqueIdentifier[..6]}";
            Current = new PlayerProfileData
            {
                PlayerId = guestId,
                Username = guestId,
                XP = 0,
                Coins = 0,
                ProgressLevel = 1
            };
        }

        public static async Task LoadAsync(bool useCloud, CancellationToken cancellationToken = default)
        {
            if (useCloud)
            {
                var cloudProfile = await CloudPlayerProfileHandler.LoadAsync(cancellationToken);
                if (cloudProfile != null)
                {
                    Current = cloudProfile;
                    Debug.Log("Loaded profile from cloud.");
                    return;
                }
                Debug.Log("No cloud profile found, falling back to guest.");
            }

            var local = LocalPlayerProfileHandler.Load();
            if (local != null)
            {
                Current = local;
                Debug.Log("Loaded profile from local.");
            }
            else
            {
                CreateGuestProfile();
                Debug.Log("Created guest profile.");
            }
        }

        public static async Task SaveAsync(bool toCloud, CancellationToken cancellationToken = default)
        {
            if (Current == null)
            {
                Debug.LogWarning("No current profile to save.");
                return;
            }

            if (toCloud)
            {
                await CloudPlayerProfileHandler.SaveAsync(Current, cancellationToken);
                Debug.Log("Saved profile to cloud.");
            }
            else
            {
                LocalPlayerProfileHandler.Save(Current);
                Debug.Log("Saved profile locally.");
            }
        }

        public static void Clear()
        {
            Current = null;
        }
    }
}
