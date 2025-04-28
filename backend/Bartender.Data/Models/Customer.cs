using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartender.Data.Models;

[Table("customers")]
public class Customer : BaseEntity
{
    [Required]
    [StringLength(20)]
    [Column("username")]
    public required string Username { get; set; }

    [Required]
    [Column("password")]
    public required string Password { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
