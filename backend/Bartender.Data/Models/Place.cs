using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

[Table("places")]
public class Place
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("business_id")]
    public int BusinessId { get; set; }

    [ForeignKey("BusinessId")] // Should reference the actual foreign key field
    public Business? Business { get; set; }

    [Column("address")]
    public required string Address { get; set; }

    [Column("city_id")]
    public int CityId { get; set; }

    [ForeignKey("CityId")] 
    public City? City { get; set; }

    [Column("opensat")]
    public TimeOnly OpensAt { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    [Column("closesat")]
    public TimeOnly ClosesAt { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    public ICollection<Staff>? Staffs { get; set; }
    public ICollection<Table>? Tables { get; set; }
    public ICollection<MenuItem>? MenuItems { get; set; }
}
