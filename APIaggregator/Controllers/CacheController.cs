using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace APIaggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController: ControllerBase
    {
        private readonly IMemoryCache _cache;

        public CacheController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpPost("clear")]
        public IActionResult ClearCache()
        {
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0); // Removes all entries
                return Ok(new { message = "In-memory cache cleared." });
            }

            return StatusCode(500, "Cache clearing not supported.");
        }
    }
}
