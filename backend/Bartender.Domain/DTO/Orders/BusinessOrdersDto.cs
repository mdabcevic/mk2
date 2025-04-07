
namespace Bartender.Domain.DTO.Orders;

public class BusinessOrdersDto
{
    public PlaceDto Place { get; set; }
    public List<OrderDto> Orders { get; set; }
}
