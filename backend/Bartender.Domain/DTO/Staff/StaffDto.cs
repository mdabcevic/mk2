using Bartender.Data.Enums;

namespace Bartender.Domain.DTO.Staff;

public class StaffDto
{
    public required string OIB { get; set; }

    public required string Username { get; set; }

    public required string FullName { get; set; }

    public EmployeeRole Role { get; set; } = EmployeeRole.regular;
}
