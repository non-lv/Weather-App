using System.ComponentModel.DataAnnotations;

namespace Weather_App.Server.Database.Models;

public class WeatherLog
{
    [Key]
    public int Id { get; set; }

    public required string Country { get; init; }
    public required string City { get; init; }
    public int Temp { get; init; }
    public int TempMin { get; init; }
    public int TempMax { get; init; }
    public long UnixTimeSeconds { get; init; }
}