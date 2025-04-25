using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RedGaint.Network.Runtime
{
    public static class UserCredentialManager
    {
        private const string UsernameKey = "auth_username";
        private const string EncryptedPasswordKey = "auth_password_enc";

        // --- Save Credentials ---
        public static void SaveCredentials(string username, string password, string encryptionKey)
        {
            string encryptedPassword = Encrypt(password, encryptionKey);
            PlayerPrefs.SetString(UsernameKey, username);
            PlayerPrefs.SetString(EncryptedPasswordKey, encryptedPassword);
            PlayerPrefs.Save();
            Debug.Log(username+" Credentials saved");
        }

        // --- Try Load Credentials ---
        public static bool TryLoadCredentials(string encryptionKey, out string username, out string password)
        {
            username = null;
            password = null;

            if (!PlayerPrefs.HasKey(UsernameKey) || !PlayerPrefs.HasKey(EncryptedPasswordKey))
                return false;

            try
            {
                username = PlayerPrefs.GetString(UsernameKey);
                string encryptedPassword = PlayerPrefs.GetString(EncryptedPasswordKey);
                password = Decrypt(encryptedPassword, encryptionKey);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Credential decrypt failed: {ex.Message}");
                return false;
            }
        }

        // --- Clear Credentials ---
        public static void ClearCredentials()
        {
            PlayerPrefs.DeleteKey(UsernameKey);
            PlayerPrefs.DeleteKey(EncryptedPasswordKey);
            PlayerPrefs.Save();
        }

        // --- AES Encrypt ---
        private static string Encrypt(string plainText, string key)
        {
            using Aes aes = Aes.Create();
            aes.Key = DeriveKey(key);
            aes.GenerateIV();

            using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }

        // --- AES Decrypt ---
        private static string Decrypt(string encryptedText, string key)
        {
            byte[] fullCipher = Convert.FromBase64String(encryptedText);
            using Aes aes = Aes.Create();
            aes.Key = DeriveKey(key);

            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, iv);
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        // --- Key Derivation (from password) ---
        private static byte[] DeriveKey(string key)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
    }

}