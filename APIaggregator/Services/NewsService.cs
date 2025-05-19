using APIaggregator.Contracts;
using APIaggregator.Models.AboutNews;
using APIaggregator.Models.News;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace APIaggregator.Services
{
    public class NewsService: INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private const int MaxNewsLimit = 100;

        public NewsService(HttpClient httpClient, IConfiguration config, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _config = config;
            _cache = cache;
        }

        public async Task<NewsResult> GetEverythingAsync(string topic, int? limit = null)
        {
            var cacheKey = $"news::{topic.ToLower()}::{limit}";

            if (_cache.TryGetValue(cacheKey, out NewsResult cached))
            {
                return cached;
            }

            var result = await FetchNewsFromApiAsync(topic, limit);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

            return result;
        }

        private async Task<NewsResult> FetchNewsFromApiAsync(string topic, int? limit)
        {
            var apiKey = _config["ApiKeys:NewsApi"];

            // apiKey: The API key i was granted after registering for a developer account.
            // q: Keywords or phrases to search for in the article title and body.
            //    The complete value for q must be URL-encoded. Max length: 500 chars.
            var url = $"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(topic)}&apiKey={apiKey}";

            string? warningMessage = null;

            var httpResponse = await _httpClient.GetAsync(url);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"News API call failed: {httpResponse.StatusCode}, Body: {error}");
            }
            var newsResponse = await httpResponse.Content.ReadFromJsonAsync<NewsResponse>();
            var articles = newsResponse?.Articles ?? new List<NewsArticle>();

            if (limit.HasValue && limit.Value > MaxNewsLimit)
            {
                warningMessage = $"Limit exceeded maximum allowed ({MaxNewsLimit}). Showing top {MaxNewsLimit} results.";
                limit = MaxNewsLimit;
            }
            int appliedLimit = limit.GetValueOrDefault(articles.Count);

            return new NewsResult
            {
                Articles = articles.Take(appliedLimit).ToList(),
                ErrorMessage = warningMessage
            };
        }
    }
}
