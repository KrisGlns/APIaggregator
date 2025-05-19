namespace APIaggregator.Models.AboutWeather
{
    public class WeatherApiResponse
    {
        public List<WeatherDescription> Weather { get; set; }
        public MainInfo Main { get; set; }
    }
}
