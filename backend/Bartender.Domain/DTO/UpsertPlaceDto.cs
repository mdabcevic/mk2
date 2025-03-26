
namespace Bartender.Domain.DTO;

public class UpsertPlaceDto
{
    public required int BusinessId { get; set; }
    public required int CityId { get; set; }
    public required string Address { get; set; }
    public required string OpensAt { get; set; }
    public required string ClosesAt { get; set; }
}
