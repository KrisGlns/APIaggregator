using APIaggregator.Contracts;
using APIaggregator.Controllers;
using APIaggregator.Models;
using APIaggregator.Models.AboutNews;
using APIaggregator.Models.GitHub;
using APIaggregator.Models.News;
using APIaggregator.Models.Weather;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIaggregatorTests.ControllersTests
{
    public class AggregateControllerTests
    {
        private readonly IWeatherService _weatherService = Substitute.For<IWeatherService>();
        private readonly INewsService _newsService = Substitute.For<INewsService>();
        private readonly IGithubService _githubService = Substitute.For<IGithubService>();
        private readonly AggregateController _controller;

        public AggregateControllerTests()
        {
            _controller = new AggregateController(_weatherService, _newsService, _githubService);
        }

        [Fact]
        public async Task GetAggregatedData_ReturnsSuccess_WhenAllApisSuccess()
        {
            // Arrange
            _weatherService.GetWeatherForCityAsync(Arg.Any<string>(), TemperatureUnit.Metric)
                .Returns(new WeatherResult
                {
                    Status = ApiStatus.Success,
                    Info = new WeatherInfo
                    {
                        City = "Athens",
                        Description = "Sunny",
                        Temperature = 24,
                        Unit = "metric"
                    }
                });

            _newsService.GetEverythingAsync(Arg.Any<string>(), Arg.Any<int?>())
                .Returns(new NewsResult
                {
                    Status = ApiStatus.Success,
                    Articles = new List<NewsArticle>
                    {
                        new()
                        {
                            Title = "Test Article", 
                            Author = "Author", 
                            PublishedAt = DateTime.UtcNow
                        }
                    }
                });

            _githubService.GetReposForUserAsync(Arg.Any<string>())
               .Returns(new GithubResult
               {
                   Status = ApiStatus.Success,
                   Repositories = new List<GithubRepo> 
                   { 
                       new() 
                       { 
                           Name = "Repo", 
                           HtmlUrl = "http://github.com/repo" 
                       } 
                   }
               });

            // Act
            var result = await _controller.GetAggregatedData("Athens", ".NET", "KrisGlns", null, 2, TemperatureUnit.Metric);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = okResult.Value.Should().BeOfType<AggregatedResponse>().Subject;

            data.Weather.Status.Should().Be(ApiStatus.Success);
            data.News.Status.Should().Be(ApiStatus.Success);
            data.GithubRepos.Status.Should().Be(ApiStatus.Success);
        }

        [Fact]
        public async Task GetAggregatedData_ReturnsPartialError_WhenGitHubFails()
        {
            // Arrange
            _weatherService.GetWeatherForCityAsync(Arg.Any<string>(), TemperatureUnit.Metric)
                .Returns(new WeatherResult
                {
                    Status = ApiStatus.Success,
                    Info = new WeatherInfo
                    {
                        City = "Athens",
                        Description = "Sunny",
                        Temperature = 24,
                        Unit = "metric"
                    }
                });

            _newsService.GetEverythingAsync(Arg.Any<string>(), Arg.Any<int?>())
                .Returns(new NewsResult
                {
                    Status = ApiStatus.Success,
                    Articles = new List<NewsArticle>
                    {
                new() { Title = "News", Author = "Author", PublishedAt = DateTime.UtcNow }
                    }
                });

            _githubService.GetReposForUserAsync(Arg.Any<string>())
                .Returns(new GithubResult
                {
                    Status = ApiStatus.Error,
                    Repositories = new List<GithubRepo>(),
                    ErrorMessage = "GitHub API down"
                });

            // Act
            var result = await _controller.GetAggregatedData("Athens", ".NET", "broken-user", null, 2, TemperatureUnit.Metric);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = okResult.Value.Should().BeOfType<AggregatedResponse>().Subject;

            data.Weather.Status.Should().Be(ApiStatus.Success);
            data.News.Status.Should().Be(ApiStatus.Success);

            data.GithubRepos.Status.Should().Be(ApiStatus.Error);
            data.GithubRepos.Data.Should().BeEmpty();
            data.GithubRepos.ErrorMessage.Should().Contain("GitHub API down");
        }
    }
}
