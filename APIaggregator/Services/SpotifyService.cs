using APIaggregator.Contracts;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using APIaggregator.Models;

namespace APIaggregator.Services
{
    public class SpotifyService : ISpotifyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private string _accessToken;

        public SpotifyService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        private async Task AuthenticateAsync()
        {
            var clientId = _config["ApiKeys:SpotifyClientId"];
            var clientSecret = _config["ApiKeys:SpotifyClientSecret"];
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            request.Content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            _accessToken = json.GetProperty("access_token").GetString();
        }

        public async Task<List<Track>> SearchTracksAsync(string query)
        {
            if (string.IsNullOrEmpty(_accessToken))
                await AuthenticateAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track&limit=5");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            return json.GetProperty("tracks").GetProperty("items")
                .EnumerateArray()
                .Select(track => new Track
                {
                    Name = track.GetProperty("name").GetString(),
                    Artist = track.GetProperty("artists")[0].GetProperty("name").GetString(),
                    Album = track.GetProperty("album").GetProperty("name").GetString(),
                    Url = track.GetProperty("external_urls").GetProperty("spotify").GetString()
                }).ToList();
        }
    }
}
