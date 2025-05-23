﻿using APIaggregator.Contracts;
using APIaggregator.Models;
using APIaggregator.Models.AboutNews;
using APIaggregator.Models.GitHub;
using APIaggregator.Models.News;
using APIaggregator.Models.Weather;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace APIaggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AggregateController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly INewsService _newsService;
        private readonly IGithubService _githubService;

        public AggregateController(IWeatherService weather, INewsService news, IGithubService githubService)
        {
            _weatherService = weather;
            _newsService = news;
            _githubService = githubService;
        }

        [HttpGet("weather")]
        public async Task<IActionResult> GetWeather(
            [FromQuery] string city, 
            [FromQuery] TemperatureUnit unit = TemperatureUnit.Metric)
        {
            var response = new ApiSection<WeatherInfo?>();

            try
            {
                var result = await _weatherService.GetWeatherForCityAsync(city, unit);
                response.Data = result.Info;
                response.Status = result.Status;

                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    response.ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                response.Status = ApiStatus.Error;
                response.ErrorMessage = $"Unexpected error: {ex.Message}";
            }

            return Ok(response);
        }

        [HttpGet("news")]
        public async Task<IActionResult> GetNews(
            [FromQuery] string topic, 
            [FromQuery] string? sort = null,
            [FromQuery] int? limit = null)
        {
            var response = new ApiSection<List<NewsArticle>>() { Data = new List<NewsArticle>() };

            try
            {
                var result = await _newsService.GetEverythingAsync(topic, limit);
                if (result.Articles == null || result.Articles.Count == 0)
                {
                    response.Status = ApiStatus.Error;
                    response.ErrorMessage = "No information found for the requested topic";
                }
                else
                {
                    response.Data = !string.IsNullOrWhiteSpace(sort) 
                        ? SortNews(result.Articles, sort) 
                        : result.Articles;

                    if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                    {
                        response.ErrorMessage = result.ErrorMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = ApiStatus.Error;
                response.ErrorMessage = $"Error fetching news data: {ex.Message}";
            }

            return Ok(response);
        }

        [HttpGet("github")]
        public async Task<IActionResult> GetGithubRepos([FromQuery] string username)
        {
            var response = new ApiSection<List<GithubRepo>>() { Data = new List<GithubRepo>() };

            try
            {
                var repos = await _githubService.GetReposForUserAsync(username);

                response.Data = repos.Repositories;
                response.Status = repos.Status;

                if (!string.IsNullOrWhiteSpace(repos.ErrorMessage))
                {
                    response.ErrorMessage = repos.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                response.Status = ApiStatus.Error;
                response.ErrorMessage = $"Error fetching GitHub repos: {ex.Message}";
            }

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAggregatedData(
            [FromQuery, DefaultValue("Athens")] string? city,
            [FromQuery, DefaultValue(".NET Core")] string? topic, 
            [FromQuery, DefaultValue("KrisGlns")] string? githubUsername,
            [FromQuery] string? sortNews = null,
            [FromQuery, Range(1, 100)] int? newsLimit = null,
            [FromQuery] TemperatureUnit unit = TemperatureUnit.Metric)
        {
            var weatherTask = _weatherService.GetWeatherForCityAsync(city, unit);
            var newsTask = _newsService.GetEverythingAsync(topic, newsLimit);
            var githubTask = _githubService.GetReposForUserAsync(githubUsername);

            await Task.WhenAll(weatherTask, newsTask, githubTask);

            var weatherResult = HandleWeatherResult(weatherTask);
            
            var newsResult = HandleNewsResult(newsTask, sortNews);
            
            var githubResult = HandleGithubResult(githubTask);
            
            var result = new AggregatedResponse
            {
                Weather = weatherResult,
                News = newsResult,
                GithubRepos = githubResult
            };

            return Ok(result);
        }

        private ApiSection<List<GithubRepo>> HandleGithubResult(Task<GithubResult> githubTask)
        {
            var result = new ApiSection<List<GithubRepo>>() { Data = new List<GithubRepo>() };

            if (githubTask.IsCompletedSuccessfully)
            {
                var githubData = githubTask.Result;
                result.Data = githubData.Repositories;
                result.Status = githubData.Status;
                result.ErrorMessage = githubData.ErrorMessage;
            }
            else
            {
                result.Status = ApiStatus.Error;
                result.ErrorMessage = githubTask.Exception?.InnerException?.Message ?? "Unknown error";
            }

            return result;
        }

        private ApiSection<List<NewsArticle>> HandleNewsResult(Task<NewsResult> newsTask, string? sortNews)
        {
            var result = new ApiSection<List<NewsArticle>>() { Data = new List<NewsArticle>() };

            if (newsTask.IsCompletedSuccessfully)
            {
                var newsResultData = newsTask.Result;
                result.Data = !string.IsNullOrEmpty(sortNews)
                    ? SortNews(newsResultData.Articles, sortNews)
                    : newsResultData.Articles;

                if (!string.IsNullOrWhiteSpace(newsResultData.ErrorMessage))
                {
                    result.ErrorMessage = newsResultData.ErrorMessage;
                }
            }
            else
            {
                result.Status = ApiStatus.Error;
                result.ErrorMessage = newsTask.Exception?.InnerException?.Message ?? "Unknown error";
            }

            return result;
        }

        private ApiSection<WeatherInfo?> HandleWeatherResult(Task<WeatherResult> weatherTask)
        {
            var result = new ApiSection<WeatherInfo?>();

            if (weatherTask.IsCompletedSuccessfully)
            {
                var weatherData = weatherTask.Result;
                result.Data = weatherData.Info;
                result.Status = weatherData.Status;
                result.ErrorMessage = weatherData.ErrorMessage;
            }
            else
            {
                result.Status = ApiStatus.Error;
                result.ErrorMessage = weatherTask.Exception?.InnerException?.Message ?? "Unknown error";
            }

            return result;
        }

        private List<NewsArticle> SortNews(List<NewsArticle> newsResult, string? sort)
        {
            return sort switch
            {
                "date" => newsResult.OrderByDescending(n => n.PublishedAt).ToList(),
                "author" => newsResult.OrderBy(n => n.Author).ToList(),
                _ => newsResult
            };
        }
    }
}
