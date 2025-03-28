
namespace Bartender.Domain.DTO;

public class UpsertBusinessDto
{
    public required string OIB { get; set; }

    public required string Name { get; set; }

    public string? Headquarters { get; set; }
}
