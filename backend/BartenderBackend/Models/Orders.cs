using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BartenderBackend.Models;

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

    [Column("status")]
    public string Status { get; set; } = "created";

    [Column("paymenttype")]
    public string PaymentType { get; set; } = "cash";

    public ICollection<ProductsPerOrder> Products { get; set; } = [];
}
