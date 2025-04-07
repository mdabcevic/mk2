using Bartender.Data.Enums;
using Bartender.Data.Models;

namespace Bartender.Domain.DTO.Orders;

public class OrderBaseDto
{
    public required decimal TotalPrice { get; set; }
    public required OrderStatus Status { get; set; } 
    public Customers? Customer { get; set; }
    public required string CreatedAt { get; set; }
}
