using APIaggregator.Models.AboutWeather;

namespace APIaggregator.Models.Weather
{
    public class WeatherResult: BaseResult
    {
        public WeatherInfo? Info { get; set; }
    }
}
