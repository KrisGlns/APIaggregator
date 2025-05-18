using APIaggregator.Contracts;
using APIaggregator.Models;
using APIaggregator.Models.AboutWeather;
using APIaggregator.Models.Weather;
using System.Text.Json;

namespace APIaggregator.Services
{
    public class WeatherService: IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public WeatherService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<WeatherResult> GetWeatherForCityAsync(string cityName, TemperatureUnit unit)
        {
            var apiKey = _config["ApiKeys:OpenWeather"];

            try
            {
                // 1. Get city geolocation
                var geoUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={cityName}&limit=1&appid={apiKey}";
                var geoRequest = new HttpRequestMessage(HttpMethod.Get, geoUrl);
                geoRequest.Headers.Add("User-Agent", "MyAppAggregator/1.0");

                var geoResponse = await _httpClient.SendAsync(geoRequest);
                if (!geoResponse.IsSuccessStatusCode)
                {
                    var error = await geoResponse.Content.ReadAsStringAsync();
                    return new WeatherResult
                    {
                        Status = ApiStatus.Error,
                        ErrorMessage = $"Geolocation API call failed: {geoResponse.StatusCode}, Body: {error}"
                    };
                }

                var locations = await geoResponse.Content.ReadFromJsonAsync<List<GeoLocation>>();
                var location = locations?.FirstOrDefault();
                if (location == null)
                {
                    return new WeatherResult
                    {
                        Status = ApiStatus.Warning,
                        ErrorMessage = $"No location found for the city: {cityName}"
                    };
                }

                // 2. Get actual weather data using lat/lon
                var unitString = unit.ToString().ToLower();
                var weatherUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={location.Lat}&lon={location.Lon}&appid={apiKey}&units={unitString}";
                var weatherRequest = new HttpRequestMessage(HttpMethod.Get, weatherUrl);
                weatherRequest.Headers.Add("User-Agent", "MyAppAggregator/1.0");

                var weatherResponse = await _httpClient.SendAsync(weatherRequest);
                if (!weatherResponse.IsSuccessStatusCode)
                {
                    var error = await weatherResponse.Content.ReadAsStringAsync();
                    return new WeatherResult
                    {
                        Status = ApiStatus.Error,
                        ErrorMessage = $"Weather API call failed: {weatherResponse.StatusCode}, Body: {error}"
                    };
                }

                var response = await weatherResponse.Content.ReadFromJsonAsync<WeatherApiResponse>();
                if (response == null)
                {
                    return new WeatherResult
                    {
                        Status = ApiStatus.Error,
                        ErrorMessage = "Failed to deserialize weather data."
                    };
                }

                return new WeatherResult
                {
                    Info = new WeatherInfo
                    {
                        City = location.Name,
                        Description = response.Weather.FirstOrDefault()?.Description ?? "No description",
                        TemperatureCelsius = response.Main.Temp,
                        Unit = unit.ToString().ToLower()
                    }
                };
            }
            catch (Exception ex)
            {

                return new WeatherResult
                {
                    Status = ApiStatus.Error,
                    ErrorMessage = $"Unexpected error: {ex.Message}"
                };
            }
        }
    }
}
