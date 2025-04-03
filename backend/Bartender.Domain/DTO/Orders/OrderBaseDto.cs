using Bartender.Data.Enums;
using Bartender.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Orders;

public class OrderBaseDto
{
    public UpsertTableDto Table { get; set; }
    public Customers? Customer { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required OrderStatus Status { get; set; }
    public required decimal TotalPrice { get; set; }
    public required PaymentType PaymentType { get; set; }
}
