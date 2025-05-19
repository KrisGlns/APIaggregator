using APIaggregator.Models.Weather;

namespace APIaggregator.Contracts
{
    public interface IWeatherService
    {
        Task<WeatherResult> GetWeatherForCityAsync(string city, TemperatureUnit unit);
    }
}
