# API Aggregator

-- Introduction --
API Aggregator is a RESTful ASP.NET Core Web API that consolidates data from multiple third-party APIs into a single unified response. It provides both individual endpoints for each external service as well as an aggregated endpoint that combines weather information for a city, news headlines and Github repository data into a single response payload.
The aggregator is designed with modularity, scalability and resilience in mind:
  * Each external API integration is encapsulated in its own service
  * Common response structure and error handling provide consistent feedback to clients
  * In-memory caching is used to reduce redundunt API calls and improve performance
  * Responses are wrapped using DTOs and structured result types to ensure clarity and testability

Through this project I learned to:
  * Integrate multiple third-party APIs
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

Cache Model:
 CacheClearResponse : A structure to unify the response of the API and make it easy to test it in Unit Tests 

API General (Shared) Models:
  ApiStatus: An enum used across all result types
  BaseResult: Abstract base class for consistent result wrappers
  ApiSection<T>: Used By controllers to wrap final responses
  AggregatedResponse: The combined result of all APIs

-- Services --
The service layer is responsible for:
 * Making HTTP requests to third-party APIs
 * Parsing and validating responses
 * Applying in-memory caching
 * Wrapping results in structured response objects
 * Each service follows the interface-driven design

WeatherService:
 It's purpose is to fetch current weather data for a given city and temperature unit using the OpenWeathermAP Api.
  * It uses OpenWeather's geocoding API to translate a city name into coordinates
  * Calls the current weather API with the coordinates and selected unit
  * Caches the result in memory for 5 minutes
  * Returns a structured WeatherResult with Status, Info, and optional ErrorMessage

NewsService:
 It's purpose is to search for news articles from NewsAPI based on topic and optional limit.
  * Sends a query to https://nwesapi.org/v2/everything?q={topic}
  * Applies limit to the number of articles (with max cap like 100)
  * Caches each result for 10 minutes
  * Returns NewsResult with Articles, Status and optional ErrorMessage

GithubService:
 It's purpose is to fetch a list of public repos for a given GitHub username.
  * Calls https://api.github.com/users/{username}/repos
  * Adds a Bearer token from configuration for higher rate limits
  * Caches result for 10 minutes

-- Controllers --
Controllers expose the application functionality as HTTP API endpoints, mapping client requests to service calls, applying validation, formatting responses, and ensuring consistent API structure.
This API exposes endpoints through a single controller: AggregateController. A secondary controller is used to clear cache: CacheController
AggregateController controller centralizes the logic for:
 * Weather data
 * News aggregation
 * Github repos
 * Combined results
Each endpoint returns a unified ApiSection<T> object with consistent structure
 * /api/aggregate/weather: Fetches current weather data for a given city (mandatory) and temperature unit (optional)
 * /api/aggregate/news: Fetches news articles from NewsAPI.org by topic (mandatory), with optional sorting and limit
 * api/aggregate/github: Fetches public repositories for a GitHub username (mandatory)
 * api/aggregate: Fetches aggregated data from all 3 APIs in a single response

-- Aggregation Logic--
The /api/aggregate endpoint consolidates data from 3 endpoints. This logic is executed asynchronously, in paraller, and with graceful error handling
Parallel execution: await Task.WhenAll(weatherTask, newsTask, githubTask); -> This avoids blocking calls from executing one after the other, which would dramatically slow down the response
Each Task result is checked individually using IsCompletedSuccessfully. 

Response Wrapping:
 * Each API result is returned using a shared format of ApiSection<T>. These are combined int oa single AggregateResponse.

Optional sorting and filtering:
 * Before returning news results, the logic optionally limits article count via the newsLimit parameter and/or sorts articles by date or author. This happens after data is fetched.
 * 
-- Testing --
This project uses xUnit as the test framework and FluentAssertions for expressive assertions. The service and controller layers are thorougly unit tested using NSubstitute and MockHttp, ensuring reliable functionality without real external dependencies.
Also, Bogus library is used to generate randomized, realistic test data for our models, avoiding hardcoded values.
Tools and Libraries
 * xUnit -> Test framework
 * FluentAssertions -> Fluent syntax for assertions
 * NSubstitute -> Mocking service dependencies
 * RichardSzalay.MockHttp -> Mocking HTTP requests for services

-- Sceduled Jobs -- 
This project uses the Quartz nuget package to schedule a recurring background job that automatically clears the in-memory cache once every 24 hours.

-- Logging with Serilog --
This project uses the Serilog Library for structured and configurable logging.
Logging outputs
 * Console (during development)
 * Daily rolling log files in the '/Logs/' directory

-- Cloning the Project from Git Bundle --
If you are receiving this project as a .bundle, you can clone it locally using:
 * git clone Christos_Galanis_API_Aggregator.bundle APIaggregator
After cloning navigate to the poject folder: cd APIaggregator
