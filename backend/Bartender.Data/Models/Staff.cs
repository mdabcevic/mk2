using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Bartender.Data.Enums;

namespace Bartender.Data.Models;

[Table("staff")]
public class Staff : BaseEntity
{
    [Required]
    [Column("place_id")]
    public int PlaceId { get; set; }

    [ForeignKey("PlaceId")]
    public Place? Place { get; set; }

    [Required, MaxLength(11)]
    [Column("oib")]
    public required string OIB { get; set; }

    [Required]
    [Column("username")]
    public required string Username { get; set; }

    [Required]
    [Column("password")]
    public required string Password { get; set; }

    [Required]
    [Column("fullname")]
    public required string FullName { get; set; }

    [Required]
    [Column("role")]
    [EnumDataType(typeof(EmployeeRole))]
    public EmployeeRole Role { get; set; } = EmployeeRole.regular;
}
