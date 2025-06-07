using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Bartender.Data.Enums;

namespace Bartender.Data.Models;

public class WeatherData
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required DateTime DateTime { get; set; }

    [Required]
    public int CityId { get; set; }

    [ForeignKey(nameof(CityId))]
    public City? City { get; set; }

    public double? Temperature { get; set; }

    [Required]
    [Column("weather_type")]
    [EnumDataType(typeof(WeatherType))]
    public WeatherType WeatherType { get; set; } = WeatherType.unknown;
}
