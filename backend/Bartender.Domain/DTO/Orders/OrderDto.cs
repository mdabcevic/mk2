
using Bartender.Data.Enums;
using Bartender.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Orders;

public class OrderDto : OrderBaseDto
{
    public int Id { get; set; }
    // TODO - use dto for ProductsPerOrder
    public List<ProductsPerOrder> Products { get; set; } = [];
}
