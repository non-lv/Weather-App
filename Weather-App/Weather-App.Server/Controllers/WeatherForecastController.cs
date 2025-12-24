using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Weather_App.Server.Database;
using Weather_App.Server.Database.Models;
using Weather_App.Server.Models;

namespace Weather_App.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration, WeatherContext dbContext) : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherLogDisplay>> Get()
    {
        try
        {
            var cities = Environment.GetEnvironmentVariable("Cities")?.Split(',') ?? configuration.GetValue<string>("Cities")?.Split(',') ?? [];

            return (await SearchLogs(cities)).Select(x => new WeatherLogDisplay
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

    private async Task<WeatherLog[]> SearchLogs(params string[] cities)
    {
        var dt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3600 * 3;
        return await dbContext.WeatherLogs.Where(x => cities.AsEnumerable().Contains(x.City) && x.UnixTimeSeconds >= dt)
            .OrderBy(x => x.UnixTimeSeconds).ToArrayAsync();
    }
}