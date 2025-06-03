using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

public class City : BaseEntity
{
    [Required]
    public required string Name { get; set; }
    public decimal Longitude { get; set; }
    public decimal Latitude { get; set; }
    public ICollection<Place> Places { get; set; } = [];
}
