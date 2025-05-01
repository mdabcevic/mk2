using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartender.Data.Models;

public class Product : BaseEntity
{
    [Required]
    [StringLength(50)]
    public required string Name { get; set; }

    public string? Volume { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public ProductCategory Category { get; set; }

    public int? BusinessId { get; set; }

    [ForeignKey(nameof(BusinessId))]
    public Business? Business { get; set; }

    public ICollection<MenuItem>? MenuItems { get; set; }
    public ICollection<Review>? Reviews { get; set; }
}
