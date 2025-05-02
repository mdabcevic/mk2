using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

public class Place : BaseEntity
{
    [Required]
    public int BusinessId { get; set; }

    [ForeignKey(nameof(BusinessId))] // Should reference the actual foreign key field
    public Business? Business { get; set; }

    public required string Address { get; set; }

    public int CityId { get; set; }

    [ForeignKey(nameof(CityId))] 
    public City? City { get; set; }

    public string? Description { get; set; }

    public TimeOnly OpensAt { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    public TimeOnly ClosesAt { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    public ICollection<Staff>? Staffs { get; set; }
    public ICollection<Table>? Tables { get; set; }
    public ICollection<MenuItem>? MenuItems { get; set; }
    public ICollection<PlaceImage>? Images { get; set; }
}
