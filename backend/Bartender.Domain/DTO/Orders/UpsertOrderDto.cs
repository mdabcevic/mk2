using Bartender.Data.Enums;
using Bartender.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Orders;

public class UpsertOrderDto
{
    public int? Id { get; set; }
    [Required]
    public required int TableId { get; set; }
    public int? CustomerId { get; set; }
    public Guid? GuestSessionId { get; set; }
    public OrderStatus? Status { get; set; }
    public decimal TotalPrice { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? Note { get; set; }
    public required List<UpsertOrderMenuItemDto> Items { get; set; }
}