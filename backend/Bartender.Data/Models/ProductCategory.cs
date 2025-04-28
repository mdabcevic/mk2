using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartender.Data.Models;

[Table("productcategory")]
public class ProductCategory : BaseEntity
{
    [Required]
    [StringLength(50)]
    [Column("name")]
    public required string Name { get; set; }

    [Column("parentcategory_id")]
    public int? ParentCategoryId { get; set; }

    [ForeignKey("ParentCategoryId")]
    public ProductCategory? ParentCategory { get; set; }

    public ICollection<ProductCategory>? Subcategories { get; set; }
    public ICollection<Product>? Products { get; set; }

}
