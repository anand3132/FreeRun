using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;
using System.Collections.Generic;

namespace RedGaint.Network.GameSessionModule
{
    public class ServerAllocationResult
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public string LobbyId { get; set; }
    }

    public class DedicatedServerService
    {
        private readonly ILogger<DedicatedServerService> _logger;
        private readonly HttpClient _httpClient;

        private const string OAuthTokenUrl = "https://services.unity.com/v1/oauth2/token";
        private const string MultiplayAllocUrl = "https://services.unity.com/multiplay/allocations";

        private const string ClientId = "844fe6c8-3c8a-4e78-b244-2858e34c1985";
        private const string ClientSecret = "SQmJFTv_tmhz9w4Yq4ikzMjeknOPhpKp";
        private const string BuildConfigId = "1293114";
        private const string RegionId = "f1697338-ae9d-4f27-b6b6-22c6e4458ae1";

        public DedicatedServerService(ILogger<DedicatedServerService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<ServerAllocationResult> StartTheServer(IExecutionContext ctx, string lobbyId)
        {
            _logger.LogInformation($"Requesting dedicated server for Lobby: {lobbyId}");

            string token = await GetAccessTokenAsync();

            var requestBody = new
            {
                buildConfigurationId = BuildConfigId,
                regionId = RegionId,
                sessionId = lobbyId,
                properties = new { mode = "standard" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, MultiplayAllocUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to allocate server: {responseBody}");
                throw new System.Exception($"Multiplay Allocation Failed: {response.StatusCode}");
            }

            _logger.LogInformation($"Server allocated successfully: {responseBody}");

            var json = JsonDocument.Parse(responseBody);
            var server = json.RootElement.GetProperty("server");

            var ip = server.GetProperty("ip").GetString();
            var port = server.GetProperty("port").GetInt32();

            return new ServerAllocationResult
            {
                Ip = ip,
                Port = port,
                LobbyId = lobbyId
            };
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var authContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret)
            });

            var response = await _httpClient.PostAsync(OAuthTokenUrl, authContent);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Token request failed: {responseString}");
                throw new System.Exception("OAuth token fetch failed");
            }

            var json = JsonDocument.Parse(responseString);
            return json.RootElement.GetProperty("access_token").GetString();
        }
    }
}
