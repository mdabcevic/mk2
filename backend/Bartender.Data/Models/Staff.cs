using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Bartender.Data.Enums;

namespace Bartender.Data.Models;

public class Staff : BaseEntity
{
    [Required]
    public int PlaceId { get; set; }

    [ForeignKey(nameof(PlaceId))]
    public Place? Place { get; set; }

    [Required, MaxLength(11)]
    public required string OIB { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    public required string FullName { get; set; }

    [Required]
    [Column("role")]
    [EnumDataType(typeof(EmployeeRole))]
    public EmployeeRole Role { get; set; } = EmployeeRole.regular;
}
