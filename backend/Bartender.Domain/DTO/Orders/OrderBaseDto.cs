using Bartender.Data.Enums;
using Bartender.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Orders;

public class OrderBaseDto
{
    public required decimal TotalPrice { get; set; }
    public string Table { get; set; }
    public string? Note { get; set; }
    public required OrderStatus Status { get; set; }
    public required PaymentType PaymentType { get; set; }
    public Customers? Customer { get; set; }
    public required string CreatedAt { get; set; }
}
