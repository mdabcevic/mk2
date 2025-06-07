using Bartender.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Order;

public class UpdateOrderStatusDto
{
    [Required]
    public required OrderStatus Status { get; set; }
    public PaymentType? PaymentType { get; set; }
}
