using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Bartender.Data.Enums;

namespace Bartender.Data.Models;

[Table("orders")]
public class Order
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("table_id")]
    public int TableId { get; set; }

    [ForeignKey("TableId")]
    public Table? Table { get; set; }

    [Column("customer_id")]
    public int? CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }

    [Column("guest_session_id")]
    public Guid? GuestSessionId { get; set; }

    [ForeignKey("GuestSessionId")]
    public GuestSession? GuestSession { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

    [Required]
    [Column("status")]
    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus Status { get; set; } = OrderStatus.created;

    [Required]
    [Column("total_price", TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; } = 0.0m;

    [Required]
    [Column("paymenttype")]
    [EnumDataType(typeof(PaymentType))]
    public PaymentType PaymentType { get; set; } = PaymentType.cash;

    [Column("note")]
    public string? Note { get; set; }

    public ICollection<ProductPerOrder> Products { get; set; } = [];
}
