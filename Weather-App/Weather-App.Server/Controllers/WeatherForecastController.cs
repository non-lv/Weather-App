using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Weather_App.Server.Database;
using Weather_App.Server.Database.Models;
using Weather_App.Server.Models;

namespace Weather_App.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration, WeatherContext dbContext) : ControllerBase
    {
        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherLogDisplay> Get()
        {
            try
            {
                var cities = Environment.GetEnvironmentVariable("Cities")?.Split(',') ?? configuration.GetValue<string>("Cities")?.Split(',') ?? [];

                return SearchLogs(cities).OrderBy(x => x.UnixTimeSeconds).ToArray().Select(x => new WeatherLogDisplay
                {
                    Country = x.Country,
                    City = x.City,
                    Temp = x.Temp,
                    TempMin = x.TempMin,
                    TempMax = x.TempMax,
                    DateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(x.UnixTimeSeconds).UtcDateTime, TimeZoneInfo.Utc)
                }).ToArray();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to db");
                throw;
            }
        }

        private IQueryable<WeatherLog> SearchLogs(params string[] cities)
        {
            var predicate = PredicateBuilder.New<WeatherLog>();

            foreach (var city in cities)
            {
                var dt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3600 * 3;
                predicate = predicate.Or(p => p.City == city && p.UnixTimeSeconds >= dt);
            }
            return dbContext.WeatherLogs.Where(predicate);
        }
    }
}
