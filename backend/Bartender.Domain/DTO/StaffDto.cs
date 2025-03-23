using Bartender.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO;

public class StaffDto
{
    public required string OIB { get; set; }

    public required string Username { get; set; }

    public required string FullName { get; set; }

    public EmployeeRole Role { get; set; } = EmployeeRole.regular;
}
