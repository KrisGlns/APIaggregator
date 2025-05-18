using APIaggregator.Models.AboutNews;
using APIaggregator.Models.News;

namespace APIaggregator.Contracts
{
    public interface INewsService
    {
        Task<NewsResult> GetEverythingAsync(string keyword, int? limit = null);
        //Task<List<NewsArticle>> GetTopHeadLinesForSpecificCountryAsync(string country, int? limit = null);
    }
}
