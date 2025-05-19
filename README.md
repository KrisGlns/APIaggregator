# APIaggregator

To run this project locally, you'll need API keys for the external services:
  NewsAPI.org
  OpenWeatherMap
  GitHub API

These keys should not be stored in appsettings.json directly. Instead, use dotnet user-secrets to manage them securely:
1) Navigate to the project folder: cd path/to/APIaggregator
2) Initialize user secrets: dotnet user-secrets init
3) Set the required keys: dotnet user-secrets set "ApiKeys:NewsApi" "your-news-api-key"
                          dotnet user-secrets set "ApiKeys:OpenWeather" "your-openweather-api-key"
                          dotnet user-secrets set "ApiKeys:GitHub" "ghp_your-github-token"
