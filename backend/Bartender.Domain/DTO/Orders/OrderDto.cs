
using Bartender.Data.Enums;
using Bartender.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Orders;

public class OrderDto : OrderBaseDto
{
    public required int Id { get; set; }
    public required List<OrderItemsDto> Items { get; set; }
}
