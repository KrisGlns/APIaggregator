using APIaggregator.Contracts;
using APIaggregator.Controllers;
using APIaggregator.Models;
using APIaggregator.Models.AboutNews;
using APIaggregator.Models.GitHub;
using APIaggregator.Models.News;
using APIaggregator.Models.Weather;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace APIaggregatorTests.ControllersTests
{
    public class AggregateControllerTests
    {
        private readonly IWeatherService _weatherService = Substitute.For<IWeatherService>();
        private readonly INewsService _newsService = Substitute.For<INewsService>();
        private readonly IGithubService _githubService = Substitute.For<IGithubService>();
        private readonly AggregateController _controller;

        private readonly Faker<WeatherInfo> _weatherFaker;
        private readonly Faker<NewsArticle> _newsFaker;
        private readonly Faker<GithubRepo> _githubFaker;

        public AggregateControllerTests()
        {
            _controller = new AggregateController(_weatherService, _newsService, _githubService);

            _weatherFaker = new Faker<WeatherInfo>()
                .RuleFor(w => w.City, f => f.Address.City())
                .RuleFor(w => w.Description, f => f.Lorem.Word())
                .RuleFor(w => w.Temperature, f => f.Random.Double(10, 35))
                .RuleFor(w => w.Unit, "metric");

            _newsFaker = new Faker<NewsArticle>()
                .RuleFor(n => n.Title, f => f.Lorem.Sentence())
                .RuleFor(n => n.Author, f => f.Name.FullName())
                .RuleFor(n => n.PublishedAt, f => f.Date.Past());

            _githubFaker = new Faker<GithubRepo>()
                .RuleFor(r => r.Name, f => f.Internet.DomainWord())
                .RuleFor(r => r.HtmlUrl, f => f.Internet.Url())
                .RuleFor(r => r.Description, f => f.Lorem.Sentence());
        }

        [Fact]
        public async Task GetAggregatedData_ReturnsSuccess_WhenAllApisSuccess()
        {
            // Arrange

            var fakeWeather = _weatherFaker.Generate();
            var fakeArticles = _newsFaker.Generate(3);
            var fakeRepos = _githubFaker.Generate(2);

            _weatherService.GetWeatherForCityAsync(Arg.Any<string>(), TemperatureUnit.Metric)
                .Returns(new WeatherResult
                {
                    Status = ApiStatus.Success,
                    Info = fakeWeather
                    //Info = new WeatherInfo
                    //{
                    //    City = "Athens",
                    //    Description = "Sunny",
                    //    Temperature = 24,
                    //    Unit = "metric"
                    //}
                });

            _newsService.GetEverythingAsync(Arg.Any<string>(), Arg.Any<int?>())
                .Returns(new NewsResult
                {
                    Status = ApiStatus.Success,
                    Articles = fakeArticles
                    //Articles = new List<NewsArticle>
                    //{
                    //    new()
                    //    {
                    //        Title = "Test Article", 
                    //        Author = "Author", 
                    //        PublishedAt = DateTime.UtcNow
                    //    }
                    //}
                });

            _githubService.GetReposForUserAsync(Arg.Any<string>())
               .Returns(new GithubResult
               {
                   Status = ApiStatus.Success,
                   Repositories = fakeRepos
                   //Repositories = new List<GithubRepo> 
                   //{ 
                   //    new() 
                   //    { 
                   //        Name = "Repo", 
                   //        HtmlUrl = "http://github.com/repo" 
                   //    } 
                   //}
               });

            // Act
            var result = await _controller.GetAggregatedData("Athens", ".NET", "KrisGlns", "date", 2, TemperatureUnit.Metric);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = okResult.Value.Should().BeOfType<AggregatedResponse>().Subject;

            data.Weather.Status.Should().Be(ApiStatus.Success);
            data.Weather.Data.Should().BeEquivalentTo(fakeWeather);

            data.News.Status.Should().Be(ApiStatus.Success);
            data.News.Data.Should().HaveCount(3);
            data.News.Data.Should().BeEquivalentTo(
                fakeArticles.OrderByDescending(n => n.PublishedAt).ToList(),
                options => options.WithStrictOrdering());

            data.GithubRepos.Status.Should().Be(ApiStatus.Success);
            data.GithubRepos.Data.Should().BeEquivalentTo(fakeRepos);
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
