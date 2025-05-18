using APIaggregator.Models;
using APIaggregator.Models.AboutNews;
using APIaggregator.Models.AboutWeather;
using APIaggregator.Models.GitHub;
using static System.Net.Mime.MediaTypeNames;

namespace APIaggregator
{
    public class AggregatedResponse
    {
        public ApiSection<WeatherInfo?> Weather { get; set; } = new();
        public ApiSection<List<NewsArticle>> News { get; set; } = new();
        public ApiSection<List<GithubRepo>> GithubRepos { get; set; } = new();
    }

    public class ApiSection<T> : BaseResult
    {
        public T? Data { get; set; }
    }
}
