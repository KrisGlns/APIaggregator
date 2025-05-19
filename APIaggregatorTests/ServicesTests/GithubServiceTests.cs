using APIaggregator.Models;
using APIaggregator.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace APIaggregatorTests.ServicesTests
{
    public class GithubServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly GithubService _service;

        public GithubServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            var client = new HttpClient(_mockHttp);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                { "ApiKeys:GitHub", "test-api-key" }
                })
                .Build();
            _service = new GithubService(client, new MemoryCache(new MemoryCacheOptions()), config);
        }

        [Fact]
        public async Task GetReposForUserAsync_ReturnsRepos_WhenUserExists()
        {
            // Arrange
            var username = "testuser";
            _mockHttp.When($"https://api.github.com/users/{username}/repos")
                .Respond("application/json", @"[
                { ""name"": ""repo1"", ""html_url"": ""http://github.com/testuser/repo1"", ""description"": ""Test Repo"", ""stargazers_count"": 5 }
            ]");

            // Act
            var result = await _service.GetReposForUserAsync(username);

            // Assert
            result.Status.Should().Be(ApiStatus.Success);
            result.Repositories.Should().HaveCount(1);
            result.Repositories[0].Name.Should().Be("repo1");
        }

        [Fact]
        public async Task GetReposForUserAsync_ReturnsWarning_WhenUserNotFound()
        {
            // Arrange
            var username = "notfound";
            _mockHttp.When($"https://api.github.com/users/{username}/repos")
                .Respond(HttpStatusCode.NotFound, "application/json", @"{""message"":""Not Found""}");

            // Act
            var result = await _service.GetReposForUserAsync(username);

            // Assert
            result.Status.Should().Be(ApiStatus.Warning);
            result.Repositories.Should().BeEmpty();
            //result.ErrorMessage.Should().Contain("not found", StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetReposForUserAsync_ReturnsError_OnException()
        {
            // Arrange
            var username = "erroruser";
            _mockHttp.When($"https://api.github.com/users/{username}/repos")
                .Throw(new HttpRequestException("Timeout"));

            // Act
            var result = await _service.GetReposForUserAsync(username);

            // Assert
            result.Status.Should().Be(ApiStatus.Error);
            result.ErrorMessage.Should().Contain("Timeout");
        }
    }
}
