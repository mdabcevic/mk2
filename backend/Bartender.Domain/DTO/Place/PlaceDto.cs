namespace Bartender.Domain.DTO.Place;

public  class PlaceDto
{
    public int Id { get; set; }
    public string? BusinessName { get; set; }
    public required string Address { get; set; }
    public required string CityName { get; set; }
    public required string WorkHours { get; set; }
}
