using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedGaint.Network.GameSessionModule
{
    public class AuthService
    {
        // private const string ClientId = "844fe6c8-3c8a-4e78-b244-2858e34c1985";
        // private const string ClientSecret = "SQmJFTv_tmhz9w4Yq4ikzMjeknOPhpKp";
        // private const string TokenExchangeUrl =
        //     "https://services.api.unity.com/auth/v1/token-exchange?projectId=52b8288e-8da7-4625-a2a3-32a577389bd1&environmentId=aacaf31c-924c-4dee-b713-e99e306445b9";

        public async Task<string> GetAccessTokenAsync()
        {
            var httpClient = new HttpClient();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ServerConfig.ClientId}:{ServerConfig.ClientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, ServerConfig.TokenExchangeUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Token request failed: {responseBody}");

            var json = JsonDocument.Parse(responseBody);
            return json.RootElement.GetProperty("accessToken").GetString();
        }
    }
}