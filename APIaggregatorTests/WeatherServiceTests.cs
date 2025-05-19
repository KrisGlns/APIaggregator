using APIaggregator.Models.Weather;
using APIaggregator.Models;
using APIaggregator.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace APIaggregatorTests
{
    public class WeatherServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly IMemoryCache _memoryCache;
        private readonly WeatherService _service;

        public WeatherServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();

            var client = new HttpClient(_mockHttp);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                { "ApiKeys:OpenWeather", "test-api-key" }
                })
                .Build();

            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _service = new WeatherService(client, config, _memoryCache);
        }

        [Fact]
        public async Task GetWeatherForCityAsync_ReturnsWeatherResult_WhenCityIsValid()
        {
            // Arrange
            var city = "London";
            var unit = TemperatureUnit.Metric;

            // Mock Geolocation API
            _mockHttp.When($"http://api.openweathermap.org/geo/1.0/direct*")
                .Respond("application/json", "[{ \"name\": \"London\", \"lat\": 51.51, \"lon\": -0.13 }]");

            // Mock Weather API
            _mockHttp.When($"https://api.openweathermap.org/data/2.5/weather*")
                .Respond("application/json", @"{
                ""weather"": [{ ""description"": ""clear sky"" }],
                ""main"": { ""temp"": 15.0 }
            }");

            // Act
            var result = await _service.GetWeatherForCityAsync(city, unit);

            // Assert
            result.Status.Should().Be(ApiStatus.Success);
            result.Info.Should().NotBeNull();
            result.Info.City.Should().Be("London");
            result.Info.Temperature.Should().Be(15.0);
            result.Info.Description.Should().Be("clear sky");
        }
    }
}
