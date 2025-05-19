using APIaggregator.Contracts;
using APIaggregator.Models.Cache;
using APIaggregator.Services;
using Polly;
using Quartz;
using Serilog;
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
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddMemoryCache();

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("ClearCacheJob");
    q.AddJob<ClearCacheJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ClearCacheJob-trigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInHours(24) //run once every 24 hours starting from app startup time
            .RepeatForever()));
        //.WithCronSchedule("0 0 0 * * ?")); //Run at 00:00:00 (midnight) every day.
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddHttpClient<INewsService, NewsService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "MyAppAggregator/1.0");
}).AddTransientHttpErrorPolicy(p => p.RetryAsync(3));
builder.Services.AddHttpClient<IGithubService, GithubService>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
