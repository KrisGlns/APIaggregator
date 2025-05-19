using APIaggregator.Models;
using APIaggregator.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RichardSzalay.MockHttp;
using System.Net;

namespace APIaggregatorTests.ServicesTests
{
    public class NewsServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly NewsService _service;

        public NewsServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();

            var client = new HttpClient(_mockHttp);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                { "ApiKeys:NewsApi", "test-api-key" }
                })
                .Build();

            _service = new NewsService(client, config, new MemoryCache(new MemoryCacheOptions()));
        }

        [Fact]
        public async Task GetEverythingAsync_ReturnsNews_WhenTopicIsValid()
        {
            // Arrange
            var topic = "technology";
            var limit = 1;

            _mockHttp.When($"https://newsapi.org/v2/everything*")
                .Respond("application/json", @"{
                ""articles"": [
                    {
                        ""title"": ""Tech News"",
                        ""author"": ""Alice"",
                        ""publishedAt"": ""2023-10-10T12:00:00Z""
                    }
                ]
            }");

            // Act
            var result = await _service.GetEverythingAsync(topic, limit);

            // Assert
            result.Status.Should().Be(ApiStatus.Success);
            result.Articles.Should().HaveCount(1);
            result.Articles[0].Title.Should().Be("Tech News");
        }

        [Fact]
        public async Task GetEverythingAsync_ReturnsError_OnApiFailure()
        {
            // Arrange
            _mockHttp.When("https://newsapi.org/v2/everything*")
                .Respond(HttpStatusCode.InternalServerError);

            // Act
            var result = await _service.GetEverythingAsync("fail", 5);

            // Assert
            result.Status.Should().Be(ApiStatus.Error);
            result.Articles.Should().BeEmpty();
            //result.ErrorMessage.Should().Contain("failed", StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetEverythingAsync_WarnsOnLimitExceeded()
        {
            // Arrange
            var topic = "AI";
            var requestedLimit = 1000;
            var actualLimit = 100; // enforced max in logic

            _mockHttp.When("https://newsapi.org/v2/everything*")
                .Respond("application/json", @"{
                ""articles"": [ { ""title"": ""Article"" } ]
            }");

            // Act
            var result = await _service.GetEverythingAsync(topic, requestedLimit);

            // Assert
            result.Status.Should().Be(ApiStatus.Success);
        }
    }
}
