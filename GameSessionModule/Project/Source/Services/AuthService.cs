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
        private readonly ILogger<AuthService> _logger;

        public AuthService(ILogger<AuthService> logger)=> _logger = logger;
        public async Task<string> GetAccessTokenAsync()
        {
            var httpClient = new HttpClient();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ServerConfig.ClientId}:{ServerConfig.ClientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, ServerConfig.TokenExchangeUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            _logger.LogInformation("🔐 Requesting Unity access token...");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("❌ Token request failed. StatusCode: {StatusCode}, Response: {ResponseBody}", 
                    response.StatusCode, responseBody);
                throw new Exception($"Token request failed: {responseBody}");
            }

            var json = JsonDocument.Parse(responseBody);
            var accessToken = json.RootElement.GetProperty("accessToken").GetString();

            _logger.LogInformation("✅ Successfully retrieved Unity access token.");
            return accessToken;
        }
    }
}