namespace Weather_App.Server.Models
{
    public class WeatherLogDisplay
    {
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public int Temp { get; set; }
        public int TempMin { get; set; }
        public int TempMax { get; set; }
        public DateTime DateTime { get; set; }
    }
}
