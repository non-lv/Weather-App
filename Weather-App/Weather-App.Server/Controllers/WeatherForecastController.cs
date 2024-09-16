using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Weather_App.Server.Database;
using Weather_App.Server.Database.Models;

namespace Weather_App.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration _configuration;
        private readonly WeatherContext _dbContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration, WeatherContext dbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherLog> Get()
        {
            try
            {
                var cities = Environment.GetEnvironmentVariable("Cities")?.Split(',') ?? _configuration.GetValue<string>("Cities").Split(',');
                return SearchLogs(cities).OrderBy(x => x.Timestamp).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to db");
                throw;
            }
        }

        private IQueryable<WeatherLog> SearchLogs(params string[] cities)
        {
            var predicate = PredicateBuilder.New<WeatherLog>();

            foreach (string city in cities)
            {
                var timeZone = TimeZoneInfo.GetSystemTimeZones().Where(x => x.DisplayName.Contains(city)).SingleOrDefault() ?? TimeZoneInfo.Local;
                var datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).AddHours(-1);

                predicate = predicate.Or(p => p.City == city && p.Timestamp > datetime);
            }
            return _dbContext.WeatherLogs.Where(predicate);
        }
    }
}
