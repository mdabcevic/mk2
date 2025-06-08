using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Bartender.Data.Enums;

namespace Bartender.Data.Models;

public class Order : BaseEntity
{
    [Required]
    public int TableId { get; set; }

    [ForeignKey(nameof(TableId))]
    public Table? Table { get; set; }

    public int? CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    public Guid? GuestSessionId { get; set; }

    [ForeignKey(nameof(GuestSessionId))]
    public GuestSession? GuestSession { get; set; }

    [Required]
    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus Status { get; set; } = OrderStatus.created;

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; } = 0.0m;

    [Required]
    [EnumDataType(typeof(PaymentType))]
    public PaymentType PaymentType { get; set; } = PaymentType.cash;

    public string? Note { get; set; }

    public int? WeatherId { get; set; }

    [ForeignKey(nameof(WeatherId))]
    public WeatherData? Weather { get; set; }

    public ICollection<ProductPerOrder> Products { get; set; } = [];
}
