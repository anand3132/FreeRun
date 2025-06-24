#if UNITY_EDITOR

using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using RedGaint.Network.Runtime.UserData;

namespace RedGaint.Network.Runtime
{
    public static class EditorCredentialManager
    {
        private const string FilePrefix = "credentials_";
        private const string FileExtension = ".json";

        [Serializable]
        private class CredentialData
        {
            public string Username;
            public string EncryptedPassword;
        }

        /// <summary>
        /// Generates or loads a unique player ID per Unity Editor instance using process ID.
        /// Ensures each simulated player has their own separate credential file.
        /// </summary>
        private static string GetPlayerIDForPlayMode()
        {
#if UNITY_EDITOR
            string processId = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
            string editorKey = $"LocalSimulatedPlayerID_{processId}";

            if (EditorPrefs.HasKey(editorKey))
            {
                string cachedId = EditorPrefs.GetString(editorKey);
                Debug.Log($"[GetPlayerIDForPlayMode] Loaded cached Editor Player ID: {cachedId}");
                return cachedId;
            }

            string newEditorId = $"EditorPlayer_{Guid.NewGuid().ToString().Substring(0, 8)}";
            EditorPrefs.SetString(editorKey, newEditorId);
            Debug.Log($"[GetPlayerIDForPlayMode] Generated new Editor Player ID: {newEditorId} for Process ID: {processId}");
            return newEditorId;
#else
            if (Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
                return Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;

            return SystemInfo.deviceUniqueIdentifier;
#endif
        }

        /// <summary>
        /// Gets the file path for the credentials file based on the player ID.
        /// </summary>
        private static string GetCredentialFilePath(string playerId)
        {
            string folder = Application.dataPath + "/../PlayerCredentials";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, $"{FilePrefix}{playerId}{FileExtension}");
            Debug.Log($"[GetCredentialFilePath] File path: {path}");
            return path;
        }

        /// <summary>
        /// Saves the given username and encrypted password to a credentials file.
        /// </summary>
        public static bool SaveCredentialsToFile(string username, string password, string encryptionKey)
        {
            Debug.Log("--------------------------------------------");
            Debug.Log("[SaveCredentialsToFile] Initiating credential save process...");

            if (string.IsNullOrEmpty(username))
            {
                Debug.LogWarning("[SaveCredentialsToFile] Username is null or empty.");
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("[SaveCredentialsToFile] Password is null or empty.");
                return false;
            }

            if (string.IsNullOrEmpty(encryptionKey))
            {
                Debug.LogWarning("[SaveCredentialsToFile] Encryption key is null or empty.");
                return false;
            }

            try
            {
                Debug.Log($"[SaveCredentialsToFile] Input validated. Username: {username}");

                string encryptedPassword = UserCredentialManager.Encrypt(password, encryptionKey);
                Debug.Log("[SaveCredentialsToFile] Password encrypted successfully.");

                string playerId = GetPlayerIDForPlayMode();
                Debug.Log($"[SaveCredentialsToFile] Retrieved Player ID: {playerId}");

                string filePath = GetCredentialFilePath(playerId);
                Debug.Log($"[SaveCredentialsToFile] Credential file path: {filePath}");

                var data = new CredentialData
                {
                    Username = username,
                    EncryptedPassword = encryptedPassword
                };

                string json = JsonUtility.ToJson(data, true);
                Debug.Log("[SaveCredentialsToFile] Credential data serialized to JSON.");

                File.WriteAllText(filePath, json, Encoding.UTF8);
                Debug.Log($"<color=green>[EditorCredentialFileStore] Credentials saved for '{playerId}' to: {filePath}</color>");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red>[EditorCredentialFileStore] Failed to save credentials: {ex.Message}</color>\n{ex}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to load stored credentials from the player-specific file.
        /// </summary>
        public static bool TryLoadCredentialsFromFile(string encryptionKey, out string username, out string password)
        {
            username = null;
            password = null;

            string playerId = GetPlayerIDForPlayMode();
            string filePath = GetCredentialFilePath(playerId);
            Debug.Log($"[TryLoadCredentialsFromFile] Loading credentials for Player ID: {playerId}");

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[EditorCredentialFileStore] No credentials file found at {filePath} for playerId '{playerId}'");
                return false;
            }

            try
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                var data = JsonUtility.FromJson<CredentialData>(json);
                username = data.Username;
                password = UserCredentialManager.Decrypt(data.EncryptedPassword, encryptionKey);

                Debug.Log($"[TryLoadCredentialsFromFile] Credentials loaded successfully for '{playerId}'");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EditorCredentialFileStore] Failed to load credentials: {e.Message}");
                return false;
            }
        }
    }
}

#endif
