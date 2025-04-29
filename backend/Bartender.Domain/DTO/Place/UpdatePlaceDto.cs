namespace Bartender.Domain.DTO.Place;

public class UpdatePlaceDto
{
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? OpensAt { get; set; }
    public string? ClosesAt { get; set; }
}
