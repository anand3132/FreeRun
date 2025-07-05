using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RedGaint.Network.GameSessionModule
{
    public class AuthService
    {
        public AuthService(ILogger<AuthService> logger) { }

        public async Task<string> GetAccessTokenAsync()
        {
            var httpClient = new HttpClient();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ServerConfig.ClientId}:{ServerConfig.ClientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, ServerConfig.TokenExchangeUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            CloudDebugLogger.LogInfo("🔐 Requesting Unity access token...");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                CloudDebugLogger.LogError($"❌ Token request failed. StatusCode: {response.StatusCode}, Response: {responseBody}");
                throw new Exception($"Token request failed: {responseBody}");
            }

            var json = JsonDocument.Parse(responseBody);
            var accessToken = json.RootElement.GetProperty("accessToken").GetString();

            CloudDebugLogger.LogInfo("✅ Successfully retrieved Unity access token.");
            return accessToken;
        }

        public async Task<string> LoginAnonymousAsync()
        {
            using var httpClient = new HttpClient();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://player-auth.services.api.unity.com/v1/authentication/anonymous"
            );
            request.Headers.Add("ProjectId", ServerConfig.ProjectId);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            CloudDebugLogger.LogInfo("🔐 Logging in anonymously for bot...");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                CloudDebugLogger.LogError($"❌ Anonymous login failed. StatusCode: {response.StatusCode}, Response: {responseBody}");
                throw new Exception($"Anonymous login failed: {responseBody}");
            }

            var json = JsonDocument.Parse(responseBody);
            string idToken = json.RootElement.GetProperty("idToken").GetString();

            CloudDebugLogger.LogInfo("✅ idToken received.");
            return idToken;
        }

        // public async Task<string> ExchangeIdTokenForAccessToken(string idToken)
        // {
        //     using var httpClient = new HttpClient();
        //
        //     var request = new HttpRequestMessage(
        //         HttpMethod.Post,
        //         $"https://services.api.unity.com/auth/v1/token-exchange?projectId={ServerConfig.ProjectId}&environmentId={ServerConfig.EnvironmentId}"
        //     );
        //
        //     var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ServerConfig.ClientId}:{ServerConfig.ClientSecret}"));
        //     request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        //
        //     var body = JsonSerializer.Serialize(new { token = idToken });
        //     request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        //
        //     CloudDebugLogger.LogInfo("🔄 Exchanging idToken for accessToken...");
        //
        //     var response = await httpClient.SendAsync(request);
        //     var responseBody = await response.Content.ReadAsStringAsync();
        //
        //     if (!response.IsSuccessStatusCode)
        //     {
        //         CloudDebugLogger.LogError($"❌ Token exchange failed. StatusCode: {response.StatusCode}, Response: {responseBody}");
        //         throw new Exception($"Token exchange failed: {responseBody}");
        //     }
        //
        //     var json = JsonDocument.Parse(responseBody);
        //     string accessToken = json.RootElement.GetProperty("accessToken").GetString();
        //
        //     CloudDebugLogger.LogInfo("✅ accessToken acquired.");
        //     return accessToken;
        // }
        
        public async Task<string> ExchangeIdTokenForAccessToken(string idToken)
        {
            using var httpClient = new HttpClient();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://services.api.unity.com/auth/v1/token-exchange?projectId={ServerConfig.ProjectId}&environmentId={ServerConfig.EnvironmentId}"
            );

            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ServerConfig.ClientId}:{ServerConfig.ClientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            // 🔥 FIXED: key should be idToken not token
            var body = JsonSerializer.Serialize(new { idToken = idToken });
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            CloudDebugLogger.LogInfo("🔄 Exchanging idToken for accessToken...");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                CloudDebugLogger.LogError($"❌ Token exchange failed. StatusCode: {response.StatusCode}, Response: {responseBody}");
                throw new Exception($"Token exchange failed: {responseBody}");
            }

            var json = JsonDocument.Parse(responseBody);
            string accessToken = json.RootElement.GetProperty("accessToken").GetString();

            CloudDebugLogger.LogInfo("✅ accessToken acquired.");
            return accessToken;
        }


        public async Task<string> ExtractPlayerIdFromToken(string jwt)
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid JWT token");

            var payload = parts[1];
            var jsonBytes = Convert.FromBase64String(PadBase64(payload));
            var json = JsonDocument.Parse(jsonBytes);

            return json.RootElement.GetProperty("sub").GetString(); // Unity Player ID
        }

        private string PadBase64(string base64)
        {
            return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '='); // Safe padding
        }
    }
}
