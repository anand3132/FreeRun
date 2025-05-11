#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.CloudSave;
using UnityEngine;
using System.Security.Cryptography;
namespace RedGaint.Network.Runtime.UserData
{
    public static class CloudPlayerProfileHandler
    {
        private const string Key = "game_profile";

        public static async Task SaveAsync(PlayerProfileData profileData, CancellationToken cancellationToken = default)
        {
            if (profileData == null)
                throw new ArgumentNullException(nameof(profileData));

            if (CloudSaveService.Instance?.Data == null)
                throw new InvalidOperationException("CloudSaveService is not available.");

            try
            {
                var json = JsonConvert.SerializeObject(profileData);
                var data = new Dictionary<string, object> { { Key, json } };
                await CloudSaveService.Instance.Data.ForceSaveAsync(data)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Debug.LogError($"Failed to save game profile: {ex.Message}");
                throw; // Re-throw to let caller handle it
            }
        }

        public static async Task<PlayerProfileData?> LoadAsync(CancellationToken cancellationToken = default)
        {
            if (CloudSaveService.Instance?.Data == null)
            {
                Debug.LogWarning("CloudSaveService is not available.");
                return null;
            }

            try
            {
                var keys = new HashSet<string> { Key };
                var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys) // Use the new API method
                    .ConfigureAwait(false);

                if (result.TryGetValue(Key, out var entryJson) && entryJson?.Value != null)
                {
                    var json = entryJson.Value.GetAsString(); // Get the serialized string value
                    return JsonConvert.DeserializeObject<PlayerProfileData>(json);
                }

                return null; // No saved data found
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Game profile load was cancelled.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load game profile: {ex.Message}");
                return null;
            }
        }

        public static string  GetEncryptionKeyFromCloud()
        {
           string AuthKey= PlayerPrefs.GetString("authKey");
           if (string.IsNullOrEmpty(AuthKey))
           {
               AuthKey = EncryptionKeyGenerator.GenerateNewKeyBase64();
               PlayerPrefs.SetString("authKey", AuthKey);
           }
           return AuthKey;
        }
    }
    
    public static class EncryptionKeyGenerator
    {
        public static string GenerateNewKeyBase64(int keySize = 256)
        {
            using var rng = RandomNumberGenerator.Create();
            int byteSize = keySize / 8; // 256 bits = 32 bytes

            byte[] keyBytes = new byte[byteSize];
            rng.GetBytes(keyBytes);

            return Convert.ToBase64String(keyBytes); // Store or print this key
        }
    }
}
