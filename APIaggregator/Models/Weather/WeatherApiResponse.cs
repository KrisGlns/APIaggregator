namespace APIaggregator.Models.AboutWeather
{
    public class WeatherApiResponse
    {
        public List<WeatherDescription> Weather { get; set; }
        public MainInfo Main { get; set; }
    }
}

public class WeatherDescription
{
    public string Description { get; set; }
}

public class MainInfo
{
    public double Temp { get; set; }
}