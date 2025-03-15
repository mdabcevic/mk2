using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BartenderBackend.Models;

[Table("productsperorder")]
public class ProductsPerOrder
{
    [Required]
    [Column("order_id")]
    [Key]
    public int OrderId { get; set; }

    [ForeignKey("OrderId")]
    public Orders? Order { get; set; }

    [Required]
    [Column("product_id")]
    [Key]
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Products? Product { get; set; }

    [Column("count")]
    public int Count { get; set; } = 1;
}
