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
        public AuthService(ILogger<AuthService> logger) { } // Retained to satisfy DI, even if unused

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
        
        public async Task<string> GetAnonymousPlayerTokenAsync()
        {
            var httpClient = new HttpClient();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ServerConfig.ClientId}:{ServerConfig.ClientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://player-auth.services.api.unity.com/v1/authentication/anonymous");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            CloudDebugLogger.LogInfo("🔐 Requesting anonymous bot token...");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                CloudDebugLogger.LogError($"❌ Anonymous token request failed. StatusCode: {response.StatusCode}, Response: {responseBody}");
                throw new Exception($"Anonymous bot auth failed: {responseBody}");
            }

            var json = JsonDocument.Parse(responseBody);
            var token = json.RootElement.GetProperty("accessToken").GetString();

            CloudDebugLogger.LogInfo("✅ Bot access token retrieved.");
            return token;
        }


    }
}