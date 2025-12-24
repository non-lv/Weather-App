using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Weather_App.Server.Database;
using Weather_App.Server.Models;

namespace Weather_App.Server.Services;

public class WeatherFetcher(ILogger<WeatherFetcher> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromMinutes(1);

    private readonly HttpClient _httpClient = new();
    private readonly string[] _cities = Environment.GetEnvironmentVariable("Cities")?.Split(',') ?? configuration.GetValue<string>("Cities")?.Split(',') ?? [];
    private readonly string? _weatherApiUrl = Environment.GetEnvironmentVariable("WeatherApiUrl") ?? configuration.GetValue<string>("WeatherApiUrl");
    private readonly Dictionary<string, long> _lastUpdate = new();
        
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (_weatherApiUrl is null or "") throw new ArgumentException($"{nameof(_weatherApiUrl)} cannot be empty");
        if (_cities.Length == 0) throw new ArgumentException($"{nameof(_cities)} cannot be empty");
            
        await FetchWeatherUpdates();
            
        using PeriodicTimer timer = new(_period);
        while (!cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationToken))
        {
            await FetchWeatherUpdates();
        }
    }

    private async Task FetchWeatherUpdates()
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WeatherContext>();

        var weatherForecasts = await CollectForecasts();

        var untracked = weatherForecasts.Where(x => !_lastUpdate.ContainsKey(x.name)).ToList();
        var tracked = weatherForecasts.Except(untracked).ToList();
        if (untracked.Count != 0)
        {
            var untrackedCityNames = untracked.Select(x => x.name).Distinct().ToList();
            var untrackedTimestamps = untracked.Select(x => x.dt).Distinct().ToList();

            // Checks if database already contains these entries
            var existingLogs = await dbContext.WeatherLogs
                .Where(x => untrackedCityNames.Contains(x.City) && untrackedTimestamps.Contains(x.UnixTimeSeconds))
                .Select(x => new { x.City, x.UnixTimeSeconds })
                .ToListAsync();

            var cities = existingLogs
                .Where(log => untracked.Any(u => u.name == log.City && u.dt == log.UnixTimeSeconds))
                .ToList();

            cities.ForEach(x => _lastUpdate.Add(x.City, x.UnixTimeSeconds));
            untracked.RemoveAll(x => cities.Any(y => y.City == x.name));
        }

        var toUpdate = tracked.Where(x => _lastUpdate[x.name] < x.dt).Concat(untracked);
            
        foreach (var weatherForecast in toUpdate)
        {
            _lastUpdate[weatherForecast.name] = weatherForecast.dt;
            dbContext.WeatherLogs.Add(new()
            {
                Country = weatherForecast.sys.country,
                City = weatherForecast.name,
                Temp = KelvinToC(weatherForecast.main.temp),
                TempMin = KelvinToC(weatherForecast.main.temp_min),
                TempMax = KelvinToC(weatherForecast.main.temp_max),
                UnixTimeSeconds = weatherForecast.dt
            });
        }
            
        await dbContext.SaveChangesAsync();
    }

    private async Task<List<WeatherForcast>> CollectForecasts()
    {
        var forecasts = new List<WeatherForcast>();
        foreach (var city in _cities)
        {
            WeatherForcast? weatherForecast;
            try
            {
                var uri = new Uri($"{_weatherApiUrl}{city}");
                var resp = await _httpClient.GetAsync(uri);
                var stream = await resp.Content.ReadAsStringAsync();

                weatherForecast = JsonSerializer.Deserialize<WeatherForcast>(stream);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve data from OpenWeather");
                continue;
            }

            if (weatherForecast == null)
            {
                logger.LogError("Failed to retrieve data for city: {City}", city);
                continue;
            }

            forecasts.Add(weatherForecast);
        }

        return forecasts;
    }

    private static int KelvinToC(double kelvin) => (int)Math.Round(kelvin - 273.15);
}