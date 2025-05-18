namespace APIaggregator.Models.AboutWeather
{
    public class WeatherInfo
    {
        public string City { get; set; }
        public string Description { get; set; }
        public double TemperatureCelsius { get; set; }
        public string Unit { get; set; } = "metric";
    }
}
