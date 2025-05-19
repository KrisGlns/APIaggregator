// https://en.ittrip.xyz/c-sharp/aggregator-microservice-pattern-csharp#index_id1

using APIaggregator.Contracts;
using APIaggregator.Services;
using Polly;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // To force enums to be shown as strings and not as numbers
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddHttpClient<INewsService, NewsService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "MyAppAggregator/1.0");
}).AddTransientHttpErrorPolicy(p => p.RetryAsync(3)); ;
builder.Services.AddHttpClient<IGithubService, GithubService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
