using APIaggregator.Controllers;
using APIaggregator.Models.Cache;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIaggregatorTests.ControllersTests
{
    public class CacheControllerTests
    {
        public static IEnumerable<object[]> CacheTestCases =>
        new List<object[]>
        {
            new object[]
            {
                new MemoryCache(new MemoryCacheOptions()), // real MemoryCache
                200,
                "In-memory cache cleared."
            },
            new object[]
            {
                new FakeCache(), // not a MemoryCache
                500,
                "Cache clearing not supported."
            }
        };

        [Theory]
        [MemberData(nameof(CacheTestCases))]
        public void ClearCache_ReturnsExpectedStatusAndMessage(IMemoryCache cache, int expectedStatus, string expectedMessage)
        {
            // Arrange
            var controller = new CacheController(cache);

            // Act
            var result = controller.ClearCache();

            // Assert
            if (expectedStatus == 200)
            {
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var data = ok.Value.Should().BeOfType<CacheClearResponse>().Subject;
                data.Message.Should().Be(expectedMessage);
            }
            else
            {
                var obj = result.Should().BeOfType<ObjectResult>().Subject;
                obj.StatusCode.Should().Be(expectedStatus);
                obj.Value.Should().Be(expectedMessage);
            }
        }

        //[Fact]
        //public void ClearCache_ReturnsOk_WhenMemoryCacheIsValid()
        //{
        //    // Arrange
        //    var memoryCache = Substitute.For<MemoryCache>(new MemoryCacheOptions());
        //    var controller = new CacheController(memoryCache);

        //    // Act
        //    var result = controller.ClearCache();

        //    // Assert
        //    var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        //    var responseData = okResult.Value.Should().BeOfType<CacheClearResponse>().Subject;

        //    responseData.Message.Should().Be("In-memory cache cleared.");
        //}

        //[Fact]
        //public void ClearCache_Returns500_WhenCacheIsNotMemoryCache()
        //{
        //    // Arrange 
        //    var fakeCache = new FakeCache();
        //    var controller = new CacheController(fakeCache);

        //    // Act
        //    var result = controller.ClearCache();

        //    // Assert
        //    var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        //    objectResult.StatusCode.Should().Be(500);
        //    objectResult.Value.Should().Be("Cache clearing not supported.");
        //}
    }

    class FakeCache : IMemoryCache
    {
        public ICacheEntry CreateEntry(object key) => Substitute.For<ICacheEntry>();
        public void Dispose() { }
        public void Remove(object key) { }
        public bool TryGetValue(object key, out object value)
        {
            value = null!;
            return false;
        }
    }
}
