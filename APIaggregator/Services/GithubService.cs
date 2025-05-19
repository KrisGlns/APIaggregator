using APIaggregator.Contracts;
using APIaggregator.Models;
using APIaggregator.Models.GitHub;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Http.Headers;

namespace APIaggregator.Services
{
    public class GithubService: IGithubService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly string _token;

        public GithubService(HttpClient httpClient, IMemoryCache cache, IConfiguration config)
        {
            _httpClient = httpClient;
            _cache = cache;
            _config = config;
            _token = _config["ApiKeys:GitHub"];
            _httpClient.DefaultRequestHeaders.UserAgent
                .Add(new ProductInfoHeaderValue("MyAppAggregator", "1.0"));

            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }
            
        }

        public async Task<GithubResult> GetReposForUserAsync(string username)
        {
            var cacheKey = $"github::{username.ToLower()}";

            if (_cache.TryGetValue(cacheKey, out GithubResult cached))
            {
                return cached;
            }

            var result = await FetchGithubReposFromApiAsync(username);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

            return result;
        }

        private async Task<GithubResult> FetchGithubReposFromApiAsync(string username)
        {
            var url = $"https://api.github.com/users/{username}/repos";

            try
            {
                var httpResponse = await _httpClient.GetAsync(url);
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return new GithubResult
                    {
                        Repositories = new List<GithubRepo>(),
                        Status = ApiStatus.Warning,
                        ErrorMessage = $"GitHub user '{username}' was not found."
                    };
                }

                httpResponse.EnsureSuccessStatusCode();

                var repos = await httpResponse.Content.ReadFromJsonAsync<List<GithubRepo>>();
                if (repos == null || repos.Count == 0)
                {
                    return new GithubResult
                    {
                        Repositories = new List<GithubRepo>(),
                        Status = ApiStatus.Warning,
                        ErrorMessage = $"GitHub user '{username}' has no public repositories."
                    };
                }

                return new GithubResult
                {
                    Repositories = repos
                };
            }
            catch (Exception ex)
            {
                return new GithubResult
                {
                    Repositories = new List<GithubRepo>(),
                    Status = ApiStatus.Error,
                    ErrorMessage = $"Error fetching GitHub repositories: {ex.Message}"
                };
            }
        }
    }
}
