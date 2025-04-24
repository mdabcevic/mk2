using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

[Table("reviews")]
public class Review
{
    [Required]
    [Column("product_id")]
    [Key]
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [Required]
    [Column("customer_id")]
    [Key]
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }

    [Column("rating")]
    [Required]
    public int Rating { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }
}
