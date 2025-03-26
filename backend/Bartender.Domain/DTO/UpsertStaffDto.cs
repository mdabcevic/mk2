using Bartender.Data.Enums;
using Bartender.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO;

public class UpsertStaffDto
{
    public int? Id { get; set; }

    [Required]
    public int PlaceId { get; set; }

    [Required, MaxLength(11)]
    public required string OIB { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    [EnumDataType(typeof(EmployeeRole))]
    public EmployeeRole Role { get; set; } = EmployeeRole.regular;
}
