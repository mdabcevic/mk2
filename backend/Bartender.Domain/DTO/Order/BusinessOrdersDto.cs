
using Bartender.Domain.DTO.Place;

namespace Bartender.Domain.DTO.Order;

public class BusinessOrdersDto
{
    public PlaceDto Place { get; set; } //TODO: required?
    public List<OrderDto>? Orders { get; set; }
}
