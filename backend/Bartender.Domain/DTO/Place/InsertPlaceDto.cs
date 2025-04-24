namespace Bartender.Domain.DTO.Place;

public class InsertPlaceDto
{
    public required int BusinessId { get; set; }
    public required int CityId { get; set; }
    public required string Address { get; set; }
    public string? OpensAt { get; set; }
    public string? ClosesAt { get; set; }
}
