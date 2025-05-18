using APIaggregator.Models;

namespace APIaggregator.Contracts
{
    public interface ISpotifyService
    {
        Task<List<Track>> SearchTracksAsync(string query);
    }
}
