using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
namespace RedGaint.Network.GameSessionModule
{
    public class ServerRegistry
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly HashSet<int> _allocatedServerIds = new HashSet<int>();

        // private const string ClientId = "844fe6c8-3c8a-4e78-b244-2858e34c1985";
        // private const string ClientSecret = "SQmJFTv_tmhz9w4Yq4ikzMjeknOPhpKp";
        //
        // private const string ServerListUrl =
        //     "https://services.api.unity.com/multiplay/servers/v1/projects/52b8288e-8da7-4625-a2a3-32a577389bd1/environments/aacaf31c-924c-4dee-b713-e99e306445b9/servers";

        public async Task<ServerInfo?> GetAvailableServerAsync()
        {
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ServerConfig.ClientId}:{ServerConfig.ClientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Get, ServerConfig.ServerListUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to fetch server list: {responseBody}");

            var serversJson = JsonDocument.Parse(responseBody).RootElement;

            foreach (var server in serversJson.EnumerateArray())
            {
                if (server.GetProperty("status").GetString() != "AVAILABLE")
                    continue;

                int id = server.GetProperty("id").GetInt32();
                if (_allocatedServerIds.Contains(id))
                    continue;

                _allocatedServerIds.Add(id);

                return new ServerInfo
                {
                    Id = id,
                    Ip = server.GetProperty("ip").GetString(),
                    Port = server.GetProperty("port").GetInt32()
                };
            }

            return null;
        }
    }
}
