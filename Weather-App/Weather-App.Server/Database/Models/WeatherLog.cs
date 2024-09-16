using System.ComponentModel.DataAnnotations;

namespace Weather_App.Server.Database.Models
{
    public class WeatherLog
    {
        [Key]
        public int Id { get; set; }
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public int Temp { get; set; }
        public int TempMin { get; set; }
        public int TempMax { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
