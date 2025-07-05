using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.Lobby.Model;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RedGaint.Network.GameSessionModule
{
    public class ServerRegistry
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly HashSet<int> _allocatedServerIds = new HashSet<int>();

        public ServerRegistry(ILogger<ServerRegistry> logger) { }

        public async Task<MultiplayAllocationInfo> GetAllocationDetailsAsync(string allocationId, string accessToken)
        {
            string endPoint = $"/{allocationId}";
            int maxRetries = 12;
            int delaySeconds = 10;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, ServerConfig.allocationUrl + endPoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    CloudDebugLogger.LogError($"❌ Failed to get allocation details. StatusCode: {response.StatusCode}, Response: {responseBody}");
                    throw new Exception($"❌ Failed to get allocation details: {responseBody}");
                }

                var allocationInfo = JsonSerializer.Deserialize<MultiplayAllocationInfo>(responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })!;

                if (!string.IsNullOrEmpty(allocationInfo.Ipv4))
                {
                    CloudDebugLogger.LogInfo($"✅ Retrieved server details: IP = {allocationInfo.Ipv4}, Port = {allocationInfo.GamePort}");
                    return allocationInfo;
                }

                string timestamp = DateTime.UtcNow.ToString("HH:mm:ss");
                CloudDebugLogger.LogInfo($"⚠️ > {timestamp} < Allocation found, but IP is not yet available. ipv4 : {allocationInfo.Ipv4}, port : {allocationInfo.GamePort}...");
                CloudDebugLogger.LogWarning($"⚠️ Allocation found, but IP is not yet available. Retrying in {delaySeconds}s...");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }

            // After retries
            CloudDebugLogger.LogError($"❌ Server IP address was not assigned within the expected time for allocationId: {allocationId}");
            throw new TimeoutException("Server IP was not available within 2 minutes.");
        }

        public async Task<bool> RemoveAllocationAsync(string allocationId, string bearerToken)
        {
            string endPoint = $"/{allocationId}";

            var request = new HttpRequestMessage(HttpMethod.Delete, ServerConfig.allocationUrl + endPoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                CloudDebugLogger.LogInfo($"✅ Successfully removed allocation: {allocationId}");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                CloudDebugLogger.LogError($"❌ Failed to remove allocation: {response.StatusCode}, Details: {error}");
                return false;
            }
        }

        public async Task<AllocationResponse> CreateAllocationAsync(string bearerToken, string lobbyId, List<Player> players)
        {
            var payloadObj = new AllocationPayload
            {
                LobbyId = lobbyId,
                Players = players
            };

            var payloadJson = JsonConvert.SerializeObject(payloadObj);

            var requestBody = new AllocationRequest
            {
                allocationId = Guid.NewGuid().ToString(),
                buildConfigurationId = ServerConfig.BuildConfigId,
                payload = payloadJson,
                regionId = ServerConfig.RegionId,
                restart = true
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, ServerConfig.allocationUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<AllocationResponse>(responseBody);
                CloudDebugLogger.LogInfo($"✅ Allocation created successfully: {responseBody}");
                return result!;
            }
            else
            {
                CloudDebugLogger.LogError($"❌ Failed to create allocation: {response.StatusCode}, {responseBody}");
                return null;
            }
        }
    }
}
