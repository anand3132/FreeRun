using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;
using UnityEngine.Networking;

namespace RedGaint.Network.Runtime
{
    public static class UserCredentialManager
    {
        private const string UsernameKey = "auth_username";
        private const string EncryptedPasswordKey = "auth_password_enc";

        // --- Save Credentials ---
        public static void SaveCredentials(string username, string password, string encryptionKey)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username), "Username cannot be null or empty.");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty.");

            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentNullException(nameof(encryptionKey), "Encryption key cannot be null or empty.");

            string encryptedPassword = Encrypt(password, encryptionKey);
            
            PlayerPrefs.SetString(UsernameKey, username);
            PlayerPrefs.SetString(EncryptedPasswordKey, encryptedPassword);
            PlayerPrefs.Save();
            
            Debug.Log(username + " Credentials saved");
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
        public static string Encrypt(string plainText, string key)
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
        public static string Decrypt(string encryptedText, string key)
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
        
        
        public static class PasswordGenerator
        {
            private static readonly string Lowercase = "abcdefghijklmnopqrstuvwxyz";
            private static readonly string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            private static readonly string Digits = "0123456789";
            private static readonly string Symbols = "!@#$%^&*()-_=+<>?";

            public static string Generate(int length = 8)
            {
                if (length < 8)
                    throw new ArgumentException("Password length must be at least 8 characters.");

                Random random = new Random();
                StringBuilder password = new StringBuilder();

                // Ensure at least one character from each required set
                password.Append(Lowercase[random.Next(Lowercase.Length)]);
                password.Append(Uppercase[random.Next(Uppercase.Length)]);
                password.Append(Digits[random.Next(Digits.Length)]);
                password.Append(Symbols[random.Next(Symbols.Length)]);

                // Fill the rest randomly
                string allChars = Lowercase + Uppercase + Digits + Symbols;
                for (int i = password.Length; i < length; i++)
                {
                    password.Append(allChars[random.Next(allChars.Length)]);
                }

                // Shuffle the result so predictable order doesn't leak info
                return ShuffleString(password.ToString(), random);
            }

            private static string ShuffleString(string str, Random random)
            {
                char[] array = str.ToCharArray();
                for (int i = array.Length - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (array[i], array[j]) = (array[j], array[i]);
                }
                return new string(array);
            }
        }
        
    }//UserCredentialManager

    
}//RedGaint.Network.Runtime