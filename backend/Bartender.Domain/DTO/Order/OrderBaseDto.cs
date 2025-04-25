using Bartender.Data.Enums;
using Bartender.Data.Models;

namespace Bartender.Domain.DTO.Order;

public class OrderBaseDto
{
    public required decimal TotalPrice { get; set; }
    public required OrderStatus Status { get; set; } 
    public Customer? Customer { get; set; }
    public required string CreatedAt { get; set; }
}
