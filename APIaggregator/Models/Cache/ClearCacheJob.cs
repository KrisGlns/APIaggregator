using Microsoft.Extensions.Caching.Memory;
using Quartz;

namespace APIaggregator.Models.Cache
{
    public class ClearCacheJob : IJob
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<ClearCacheJob> _logger;

        public ClearCacheJob(IMemoryCache cache, ILogger<ClearCacheJob> logger)
        {
            _cache = cache;
            _logger = logger;
        }
        public Task Execute(IJobExecutionContext context)
        {
            if(_cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
                _logger.LogInformation("[Quartz] Cache cleared at {Time}", DateTime.Now);
            }
            else
            {
                _logger.LogWarning("[Quartz] Cache instance is not a MemoryCache");
            }

                return Task.CompletedTask;
        }
    }
}
