using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

public class MenuItem : BaseEntity
{
    [Required]
    public int PlaceId { get; set; }

    [ForeignKey(nameof(PlaceId))]
    public Place? Place { get; set; }

    [Required]
    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; } = 0.0m;

    [Required]
    public bool IsAvailable { get; set; }

    public string? Description { get; set; }
}
