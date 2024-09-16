using Microsoft.EntityFrameworkCore;
using Weather_App.Server.Database.Models;

namespace Weather_App.Server.Database
{
    public class WeatherContext : DbContext
    {
        public WeatherContext(DbContextOptions<WeatherContext> options) : base(options) { }
        public DbSet<WeatherLog> WeatherLogs { get; set; }
    }
}
