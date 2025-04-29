using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace RedGaint.Network.Runtime
{
    public static class SocialLoginHelper
    {
        [Serializable]
        private class AppleUserInfo
        {
            public string email;
            public string sub;
        }

        [Serializable]
        private class GoogleUserInfo
        {
            public string sub;
            public string name;
            public string given_name;
            public string family_name;
            public string picture;
            public string email;
            public bool email_verified;
            public string locale;
        }

        [Serializable]
        private class FacebookUserInfo
        {
            public string id;
            public string name;
            public string email;
        }

        /// <summary>
        /// Extracts user's email from Apple ID token (first login only).
        /// </summary>
        public static string GetAppleUserEmail(string idToken)
        {
            if (string.IsNullOrEmpty(idToken))
            {
                Debug.LogWarning("Apple idToken is null or empty.");
                return null;
            }

            string[] parts = idToken.Split('.');
            if (parts.Length != 3)
            {
                Debug.LogWarning("Invalid Apple ID token format.");
                return null;
            }

            try
            {
                string payload = parts[1];
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                byte[] jsonBytes = Convert.FromBase64String(payload);
                string json = Encoding.UTF8.GetString(jsonBytes);

                AppleUserInfo userInfo = JsonUtility.FromJson<AppleUserInfo>(json);

                Debug.Log("Apple Email: " + userInfo.email);
                return userInfo.email;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to decode Apple token: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Fetches user's display name from Google using access token.
        /// </summary>
        public static async Task<string> GetGoogleUserNameAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                Debug.LogWarning("Google accessToken is null or empty.");
                return null;
            }

            UnityWebRequest www = UnityWebRequest.Get("https://openidconnect.googleapis.com/v1/userinfo");
            www.SetRequestHeader("Authorization", "Bearer " + accessToken);

            var asyncOp = www.SendWebRequest();
            while (!asyncOp.isDone)
                await Task.Yield();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var json = www.downloadHandler.text;
                Debug.Log("Google User Info: " + json);

                var userInfo = JsonUtility.FromJson<GoogleUserInfo>(json);
                return userInfo.name;
            }
            else
            {
                Debug.LogError("Failed to get Google user info: " + www.error);
                return null;
            }
        }

        /// <summary>
        /// Fetches user's name from Facebook using access token.
        /// </summary>
        public static async Task<string> GetFacebookUserNameAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                Debug.LogWarning("Facebook accessToken is null or empty.");
                return null;
            }

            // We can ask for fields name,email
            string url = $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}";

            UnityWebRequest www = UnityWebRequest.Get(url);

            var asyncOp = www.SendWebRequest();
            while (!asyncOp.isDone)
                await Task.Yield();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var json = www.downloadHandler.text;
                Debug.Log("Facebook User Info: " + json);

                var userInfo = JsonUtility.FromJson<FacebookUserInfo>(json);
                return userInfo.name;
            }
            else
            {
                Debug.LogError("Failed to get Facebook user info: " + www.error);
                return null;
            }
        }
    }
}
