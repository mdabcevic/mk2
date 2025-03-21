using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Bartender.Data.Enums;

namespace Bartender.Data.Models;

[Table("orders")]
public class Orders
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("table_id")]
    public int TableId { get; set; }

    [ForeignKey("TableId")]
    public Tables Table { get; set; } = new Tables();

    [Column("customer_id")]
    public int? CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customers? Customer { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    [Column("status")]
    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus Status { get; set; } = OrderStatus.created;

    [Required]
    [Column("paymenttype")]
    [EnumDataType(typeof(PaymentType))]
    public PaymentType PaymentType { get; set; } = PaymentType.cash;

    public ICollection<ProductsPerOrder> Products { get; set; } = [];
}
