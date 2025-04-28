
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

[Table("cities")]
public class City : BaseEntity
{
    [Required]
    [Column("name")]
    public required string Name { get; set; }

    public ICollection<Place> Places { get; set; } = [];

}
