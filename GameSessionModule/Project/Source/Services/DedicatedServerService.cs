using System;
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
        private static readonly HttpClient _httpClient = new HttpClient();

        private const string TokenExchangeUrl =
            "https://services.api.unity.com/auth/v1/token-exchange?projectId=52b8288e-8da7-4625-a2a3-32a577389bd1&environmentId=aacaf31c-924c-4dee-b713-e99e306445b9";

        private const string MultiplayAllocUrl = "https://services.unity.com/multiplay/allocations";

        // ⚠️ Move to Unity Cloud Code environment variables before production
        private const string ClientId = "844fe6c8-3c8a-4e78-b244-2858e34c1985";
        private const string ClientSecret = "SQmJFTv_tmhz9w4Yq4ikzMjeknOPhpKp";
        private const string BuildConfigId = "1293114";
        private const string RegionId = "f1697338-ae9d-4f27-b6b6-22c6e4458ae1";

        public DedicatedServerService(ILogger<DedicatedServerService> logger)
        {
            _logger = logger;
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
                throw new Exception($"Multiplay Allocation Failed: {response.StatusCode}");
            }

            _logger.LogInformation($"Server allocated successfully: {responseBody}");

            var json = JsonDocument.Parse(responseBody);
            var server = json.RootElement.GetProperty("server");

            return new ServerAllocationResult
            {
                Ip = server.GetProperty("ip").GetString(),
                Port = server.GetProperty("port").GetInt32(),
                LobbyId = lobbyId
            };
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, TokenExchangeUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Token request failed: {responseString}");
                throw new Exception("OAuth token fetch failed");
            }

            var json = JsonDocument.Parse(responseString);
            return json.RootElement.GetProperty("accessToken").GetString(); // ✅ For token-exchange
        }
    }
}
