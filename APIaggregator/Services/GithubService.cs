using APIaggregator.Contracts;
using APIaggregator.Models;
using APIaggregator.Models.GitHub;
using System.Net;
using System.Net.Http.Headers;

namespace APIaggregator.Services
{
    public class GithubService: IGithubService
    {
        private readonly HttpClient _httpClient;

        public GithubService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MyAppAggregator", "1.0"));
        }

        public async Task<GithubResult> GetReposForUserAsync(string username)
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
