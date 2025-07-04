using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RedGaint.Network.GameSessionModule
{
    public class HttpHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<HttpHelper> _logger;

        public HttpHelper(ILogger<HttpHelper> logger)
        {
            _logger = logger;
        }

        public async Task<JsonElement> SendGetRequestAsync(string url, Dictionary<string, string>? headers = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"GET request to {url} failed: {content}");
                throw new Exception($"GET request failed: {response.StatusCode}");
            }

            return JsonDocument.Parse(content).RootElement;
        }

        public async Task<JsonElement> SendPostRequestAsync(string url, object body, Dictionary<string, string>? headers = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };

            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"POST request to {url} failed: {content}");
                throw new Exception($"POST request failed: {response.StatusCode}");
            }

            return JsonDocument.Parse(content).RootElement;
        }
    }
}
