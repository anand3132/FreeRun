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
        
        private readonly ILogger<ServerRegistry> _logger;
        public ServerRegistry(ILogger<ServerRegistry> logger)=> _logger = logger;
        
        public async Task<MultiplayAllocationInfo> GetAllocationDetailsAsync(string allocationId, string accessToken)
        {
            string endPoint = $"/{allocationId}";

            var request = new HttpRequestMessage(HttpMethod.Get, ServerConfig.allocationUrl + endPoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("❌ Failed to get allocation details. StatusCode: {StatusCode}, Response: {ResponseBody}",
                    response.StatusCode, responseBody);

                throw new Exception($"❌ Failed to get allocation details: {responseBody}");
            }

            var allocationInfo = JsonSerializer.Deserialize<MultiplayAllocationInfo>(responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;

            _logger.LogInformation("✅ Retrieved server details: IP = {Ip}, Port = {Port}", allocationInfo.Ipv4,
                allocationInfo.GamePort);

            return allocationInfo;
        }

        public async Task<bool> RemoveAllocationAsync(string allocationId, string bearerToken)
        {
            string endPoint =$"/{allocationId}";

            var request = new HttpRequestMessage(HttpMethod.Delete, ServerConfig.allocationUrl+endPoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"✅ Successfully removed allocation: {allocationId}");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"❌ Failed to remove allocation: {response.StatusCode}, Details: {error}");
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
                _logger.LogInformation("✅ Allocation created successfully: {response}", responseBody);
                return result!;
            }
            else
            {
                _logger.LogError("❌ Failed to create allocation: {statusCode}, {response}", response.StatusCode, responseBody);
                return null;
            }
        }
    }
    
}
