using Newtonsoft.Json;
using Weather_App.Server.Database;
using Weather_App.Server.Models;

namespace Weather_App.Server.Services
{
    public class WeatherFetcher : BackgroundService
    {
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1);

        private readonly ILogger<WeatherFetcher> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly HttpClient _httpClient;
        private readonly string[] _cities;
        private readonly string _weatherApiUrl;

        public WeatherFetcher(ILogger<WeatherFetcher> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _httpClient = new HttpClient();

            _cities = configuration.GetValue<string>("Cities").Split(',');
            _weatherApiUrl = configuration.GetValue<string>("WeatherApiUrl");
        }

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
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WeatherContext>();

            foreach (var city in _cities)
            {
                WeatherForcast? weatherForcast;
                try
                {
                    var uri = new Uri($"{_weatherApiUrl}{city}");
                    var resp = await _httpClient.GetAsync(uri);
                    var stream = await resp.Content.ReadAsStringAsync();

                    weatherForcast = JsonConvert.DeserializeObject<WeatherForcast>(stream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to retrieve data from OpenWeather");
                    continue;
                }
                if (weatherForcast == null)
                {
                    _logger.LogError($"Failed to retrieve data for city: {city}");
                    continue;
                }

                var dt = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(weatherForcast.dt).UtcDateTime, TimeZoneInfo.Local);
                var ct = weatherForcast.name;

                // Duplicate entry
                if (dbContext.WeatherLogs.Where(x => x.Timestamp == dt && x.City == ct).FirstOrDefault() != null)
                    continue;

                dbContext.WeatherLogs.Add(new()
                {
                    Country = weatherForcast.sys.country,
                    City = weatherForcast.name,
                    Temp = KelvinToC(weatherForcast.main.temp),
                    TempMin = KelvinToC(weatherForcast.main.temp_min),
                    TempMax = KelvinToC(weatherForcast.main.temp_max),
                    Timestamp = dt
                });
            }
            dbContext.SaveChanges();
        }

        private static int KelvinToC(double kelvin) => (int)Math.Round(kelvin - 273.15);
    }
}
