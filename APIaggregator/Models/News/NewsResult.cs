using APIaggregator.Models.AboutNews;

namespace APIaggregator.Models.News
{
    public class NewsResult: BaseResult
    {
        public List<NewsArticle> Articles { get; set; }
    }
}
