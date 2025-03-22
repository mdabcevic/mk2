using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

[Table("places")]
public class Places
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("business_id")]
    public int BusinessId { get; set; }

    [ForeignKey("BusinessId")] // Should reference the actual foreign key field
    public Businesses? Business { get; set; }

    [Column("location")]
    public required string Location { get; set; }

    [Column("opensat")]
    public TimeOnly OpensAt { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    [Column("closesat")]
    public TimeOnly ClosesAt { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    public ICollection<Staff>? Staffs { get; set; }
    public ICollection<Tables>? Tables { get; set; }
    public ICollection<MenuItems>? MenuItems { get; set; }
}
