using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartender.Data.Models;

public class ProductCategory : BaseEntity
{
    [Required]
    [StringLength(50)]
    public required string Name { get; set; }

    public int? ParentCategoryId { get; set; }

    [ForeignKey(nameof(ParentCategoryId))]
    public ProductCategory? ParentCategory { get; set; }

    public ICollection<ProductCategory>? Subcategories { get; set; }
    public ICollection<Product>? Products { get; set; }
}
