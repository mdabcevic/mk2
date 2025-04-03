using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

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
    [Column("menuitem_id")]
    public int MenuItemId { get; set; }

    [ForeignKey("MenuItemId")]
    public MenuItems? MenuItem { get; set; }

    [Required]
    [Column("item_price", TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column("discount", TypeName = "decimal(5,2)")]
    public decimal Discount { get; set; } = 0.00m;

    [Column("count")]
    public int Count { get; set; } = 1;
}
