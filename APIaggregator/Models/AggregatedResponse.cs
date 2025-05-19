using APIaggregator.Models.AboutNews;
using APIaggregator.Models.Weather;
using APIaggregator.Models.GitHub;

namespace APIaggregator.Models
{
    public class AggregatedResponse
    {
        public ApiSection<WeatherInfo?> Weather { get; set; } = new();
        public ApiSection<List<NewsArticle>> News { get; set; } = new();
        public ApiSection<List<GithubRepo>> GithubRepos { get; set; } = new();
    }
}
