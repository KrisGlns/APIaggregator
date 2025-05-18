using APIaggregator.Models.GitHub;

namespace APIaggregator.Contracts
{
    public interface IGithubService
    {
        Task<GithubResult> GetReposForUserAsync(string username);
    }
}
