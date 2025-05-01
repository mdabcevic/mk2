using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

public class Customer : BaseEntity
{
    [Required]
    [StringLength(20)]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
