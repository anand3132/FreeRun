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
        private const string AssetFolderPath = "Assets/Resources";
        private const string AssetPath = AssetFolderPath + "/EditorPlayerId.asset";

        [Serializable]
        private class CredentialData
        {
            public string Username;
            public string EncryptedPassword;
        }

        // private static string GetEditorPlayerId()
        // {
        //     // Try load asset
        //     var asset = AssetDatabase.LoadAssetAtPath<EditorPlayerId>(AssetPath);
        //     if (asset == null)
        //     {
        //         // Create folder if it doesn't exist
        //         if (!Directory.Exists(AssetFolderPath))
        //         {
        //             Directory.CreateDirectory(AssetFolderPath);
        //         }
        //     
        //         // Create and save asset
        //         asset = ScriptableObject.CreateInstance<EditorPlayerId>();
        //         asset.playerId = Guid.NewGuid().ToString("N"); // or "D" for dashes
        //         AssetDatabase.CreateAsset(asset, AssetPath);
        //         AssetDatabase.SaveAssets();
        //         var id = GetPlayerIDForPlayMode();
        //         Debug.Log($"[EditorCredentialManager] Created new EditorPlayerId.asset with ID: {asset.playerId}");
        //     }
        //     return string.IsNullOrEmpty(asset.playerId) ? "default" : asset.playerId;
        //
        //     return GetEditorPlayerId();
        // }

        
#if UNITY_EDITOR
        private static string GetPlayerIDForPlayMode() {
            const string vpIdArg = "-vpId";
            var args = System.Environment.GetCommandLineArgs();
            foreach (var arg in args) {
                if (arg.StartsWith(vpIdArg)) {
                    return arg.Replace($"\n{vpIdArg}=", string.Empty);
                }
            }
            return string.Empty;
        }
#endif
        
        
        
        private static string GetCredentialFilePath(string playerId)
        {
            string folder = Application.dataPath + "/../PlayerCredentials";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var path= Path.Combine(folder, $"{FilePrefix}{playerId}{FileExtension}");
            Debug.Log(path);
            return path;
        }

        public static void SaveCredentialsToFile(string username, string password, string encryptionKey)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentNullException(nameof(encryptionKey));

            string encryptedPassword = UserCredentialManager.Encrypt(password, encryptionKey);
            string playerId = GetPlayerIDForPlayMode();
            Debug.Log($"Getting id : {playerId}");
            string filePath = GetCredentialFilePath(playerId);

            var data = new CredentialData
            {
                Username = username,
                EncryptedPassword = encryptedPassword
            };

            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(filePath, json, Encoding.UTF8);
            Debug.Log($"<color=red>[EditorCredentialFileStore] Credentials saved for '{playerId}' to: {filePath}</color>");
        }

        public static bool TryLoadCredentialsFromFile(string encryptionKey, out string username, out string password)
        {
            username = null;
            password = null;

            string playerId = GetPlayerIDForPlayMode();
            string filePath = GetCredentialFilePath(playerId);

            if (!File.Exists(filePath))
            {
                Debug.Log($"[EditorCredentialFileStore] No credentials file found /n {filePath} /n for playerId '{playerId}'");
                return false;
            }

            try
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                var data = JsonUtility.FromJson<CredentialData>(json);
                username = data.Username;
                password = UserCredentialManager.Decrypt(data.EncryptedPassword, encryptionKey);
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
