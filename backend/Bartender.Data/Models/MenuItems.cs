using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

[Table("menuitems")]
public class MenuItems
{
    [Required]
    [Column("place_id")]
    [Key]
    public int PlaceId { get; set; }

    [ForeignKey("PlaceId")]
    public Places? Place { get; set; }

    [Required]
    [Column("product_id")]
    [Key]
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Products? Product { get; set; }

    [Required]
    [Column("price", TypeName = "decimal(10,2)")]
    public decimal Price { get; set; } = 0.0m;

    [Required]
    [Column("isavailable")]
    public bool IsAvailable { get; set; }

    [Column("description")]
    public string? Description { get; set; }
}
