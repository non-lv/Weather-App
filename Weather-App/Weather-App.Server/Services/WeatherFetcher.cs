using System.Text.Json;
using Weather_App.Server.Database;
using Weather_App.Server.Models;

namespace Weather_App.Server.Services
{
    public class WeatherFetcher(ILogger<WeatherFetcher> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory) : BackgroundService
    {
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1);

        private readonly HttpClient _httpClient = new();
        private readonly string[] _cities = Environment.GetEnvironmentVariable("Cities")?.Split(',') ?? configuration.GetValue<string>("Cities")?.Split(',') ?? [];
        private readonly string _weatherApiUrl = Environment.GetEnvironmentVariable("WeatherApiUrl") ?? configuration.GetValue<string>("WeatherApiUrl") ?? string.Empty;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
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

                // Duplicate entry
                if (dbContext.WeatherLogs.FirstOrDefault(x => x.UnixTimeSeconds == weatherForecast.dt && x.City == weatherForecast.name) != null)
                    continue;

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

        private static int KelvinToC(double kelvin) => (int)Math.Round(kelvin - 273.15);
    }
}
