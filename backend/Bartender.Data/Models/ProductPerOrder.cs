using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

public class ProductPerOrder
{
    [Required]
    [Key]
    public int OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    [Required]
    public int MenuItemId { get; set; }

    [ForeignKey(nameof(MenuItemId))]
    public MenuItem? MenuItem { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Discount { get; set; } = 0.00m;

    public int Count { get; set; } = 1;
}
