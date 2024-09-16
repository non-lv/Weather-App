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
        private readonly WeatherContext dbContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, WeatherContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherLog> Get()
        {
            try
            {
                return dbContext.WeatherLogs.Where(x => x.Timestamp > DateTime.UtcNow.AddHours(-1)).OrderBy(x => x.Timestamp).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to db");
                throw;
            }
        }
    }
}
