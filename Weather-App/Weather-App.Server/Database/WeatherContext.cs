using Microsoft.EntityFrameworkCore;
using Weather_App.Server.Database.Models;

namespace Weather_App.Server.Database
{
    public class WeatherContext(DbContextOptions<WeatherContext> options) : DbContext(options)
    {
        public DbSet<WeatherLog> WeatherLogs { get; set; } = null!;
    }
}
