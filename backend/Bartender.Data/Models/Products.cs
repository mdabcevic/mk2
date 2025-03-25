using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartender.Data.Models;

[Table("products")]
public class Products
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column("name")]
    public required string Name { get; set; }

    [Column("volume")]
    public string? Volume { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public ProductCategory Category { get; set; }
    public ICollection<MenuItems>? MenuItems { get; set; }
    public ICollection<Reviews>? Reviews { get; set; }
}
