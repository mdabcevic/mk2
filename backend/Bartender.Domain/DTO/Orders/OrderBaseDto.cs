using Bartender.Data.Enums;
using Bartender.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Orders;

public class OrderBaseDto
{
    public int TableId { get; set; }
    // TODO - use dto for Tables
    public Tables Table { get; set; } = new Tables();
    public Customers? Customer { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public PaymentType PaymentType { get; set; }
}
