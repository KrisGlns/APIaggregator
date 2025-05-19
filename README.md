# APIaggregator

-- Introduction --
API Aggregator is a RESTful ASP.NET Core Web API that consolidates data from multiple third-party APIs into a single unified response. It provides both individual endpoints for each external service as well as an aggregated endpoint that combines weather information for a city, news headlines and Github repository data into a single response payload.
The aggregator is designed with modularity, scalability and resilience in mind:
  * Each external API integration is encapsulated in its own service
  * Common response structure and error handling provide consistent feedback to clients
  * In-memory caching is used to reduce redundunt API calls and improve performance
  * Responses are wrapped using DTOs and structured result types to ensure clarity and testability

Through this project I learned to:
  * Integrate multiple hird-party APIs
  * Structure APIs with service layers and DTOs
  * Apply in-memory caching and graceful error handling
  * Use .NET dependency injection, configuration and user secrets

-- Project Setup --
Before running the project, ensure you have:
  * .NET 8 SDK
  * Visual Studio 2022 or VS Code
  * A GitHub account
  * API keys for:
    OpenWeatherMap, NewsAPI.org, GitHub API (Personal Access Token)

Top-Level overview of the project
APIaggregator/      
  Contracts/        |  Interfaces to be implemented by Services
  Controllers/      |  API endpoints for Weather, News, GitHub, Aggregate, Cache
  Models/           |  DTOs and response models
  Services/         |  Service implementations for external APIs
  appsettings.json  |  Non-sensitive configuration (sensitive info is stored in user secrets)
  Program.cs        |  Application entry point

To run this project locally, clone the repository from github and use .NET's secure user secrets system for your API keys:
  dotnet user-secrets init
  dotnet user-secrets set "ApiKeys:OpenWeather" "your-api-key"
  dotnet user-secrets set "ApiKeys:NewsApi" "your-api-key"
  dotnet user-secrets set "ApiKeys:GitHub" "your-api-key"

Bruno - Postman Collection Instructions
  * I provide a preconfigured Bruno collection for testing all API endpoints
  * Open the API_Aggregator_Bruno.json collection
  * Import the collection to Bruno
  * Create an environment (collection environments -> configure -> name it local -> Add variable -> baseUrl = http://localhost:5185)
  * Make sure the program is running on this port
  * The same steps apply to postman configuration
NOTE: For the collection to run in bruno I have commented out the ollowing line in Program.cs: //app.UseHttpsRedirection();

-- DTOs & Models --
This project uses a clean separation of external API response models (used for deserialization) and internal applicaiton models (used for logic and response formatting).

Weather Models:
  WeatherInfo: Represents weather data returned to the client
  WeatherResult: Wrapper for service responses
  Geolocation: Used to parse coordinates from OpenWeather's geocoding API
  WeatherApiResponse (including WeatherDescription and MainInfo): Used to parse raw weather data from OpenWeather

News Models:
  NewsArticle (including NewsSource): Represents a single news article returned to the client
  NewsResult: Wrapper for processed articles
  NewsResponse: Used to parse raw data from the NewsAPI

GitHub Models:
  GithubRepo: Represents a GitHub repository
  GithubResult: Wrapper for GitHub repo responses

API General (Shared) Models:
  ApiStatus: An enum used across all result types
  BaseResult: Abstract base class for consistent result wrappers
  ApiSection<T>: Used By controllers to wrap final responses
  AggregatedResponse: The combined result of all APIs



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
