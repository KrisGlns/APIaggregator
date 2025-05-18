using APIaggregator.Contracts;
using APIaggregator.Models.AboutNews;
using APIaggregator.Models.News;
using System.Text.Json;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace APIaggregator.Services
{
    public class NewsService: INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private const int MaxNewsLimit = 100;

        public NewsService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<NewsArticle>> GetTopHeadLinesForSpecificCountryAsync(string country, int? limit = null)
        {
            var apiKey = _config["ApiKeys:NewsApi"];

            // country: The 2-letter ISO 3166-1 code of the country you want to get headlines for.
            // Possible options: ae, ar, at, au, be, bg, br, ca, ch, cn, ...
            var url = $"https://newsapi.org/v2/top-headlines?country={country}&apiKey={apiKey}";

            var httpResponse = await _httpClient.GetAsync(url);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"News API call failed: {httpResponse.StatusCode}, Body: {error}");
            }
            var news = await httpResponse.Content.ReadFromJsonAsync<NewsResponse>();
            return news.Articles ?? new List<NewsArticle>();

            //var response = await _httpClient.GetFromJsonAsync<JsonElement>(url);
            //return response.GetProperty("articles").EnumerateArray().Select(article => new NewsArticle
            //{
            //    Title = article.GetProperty("title").GetString(),
            //    Url = article.GetProperty("url").GetString(),
            //    Source = article.GetProperty("source").GetProperty("id").GetString(),
            //    PublishedAt = DateTime.Parse(article.GetProperty("publishedAt").GetString())
            //}).ToList();
        }

        public async Task<NewsResult> GetEverythingAsync(string keyword, int? limit = null)
        {
            var apiKey = _config["ApiKeys:NewsApi"];
            // apiKey: The API key i was granted after registering for a developer account.
            // q: Keywords or phrases to search for in the article title and body.
            //    The complete value for q must be URL-encoded. Max length: 500 chars.
            var url = $"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(keyword)}&apiKey={apiKey}";

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
