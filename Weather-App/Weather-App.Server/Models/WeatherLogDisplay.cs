namespace Weather_App.Server.Models;

public class WeatherLogDisplay
{
    public required string Country { get; set; }
    public required string City { get; set; }
    public int Temp { get; set; }
    public int TempMin { get; set; }
    public int TempMax { get; set; }
    public DateTime DateTime { get; set; }
}