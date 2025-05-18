using APIaggregator;
using APIaggregator.Contracts;
using APIaggregator.Controllers;
using APIaggregator.Models;
using APIaggregator.Models.AboutWeather;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static System.Net.Mime.MediaTypeNames;

namespace APIaggregatorTests
{
    public class UnitTest
    {
        [Fact]
        public async Task Get_ReturnsAggregatedData()
        {
            var weather = new WeatherInfo { City = "Athens", TemperatureCelsius = 24.5, Description = "Clear sky" };
            //var news = new List<NewsArticle> { new NewsArticle { Title = "Breaking", Source = "BBC", Url = "http://...", PublishedAt = DateTime.UtcNow } };

            var mockWeather = new Mock<IWeatherService>();
            //mockWeather.Setup(s => s.GetWeatherForCityAsync("Athens")).ReturnsAsync(weather);


            var mockNews = new Mock<INewsService>();
            //mockNews.Setup(s => s.GetEverythingAsync("Tech")).ReturnsAsync(news);

            //var controller = new AggregateController(mockWeather.Object, mockTwitter.Object, mockNews.Object);

            //var result = await controller.Get();

            //var okResult = Assert.IsType<OkObjectResult>(result.Result);
            //var data = Assert.IsType<AggregatedResponse>(okResult.Value);
            //Assert.Equal("Athens", data.Weather.City);
        }
    }
}