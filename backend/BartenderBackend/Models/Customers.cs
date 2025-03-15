using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BartenderBackend.Models;

[Table("customers")]
public class Customers
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    [Column("username")]
    public required string Username { get; set; }

    [Required]
    [Column("password")]
    public required string Password { get; set; }

    public ICollection<Orders> Orders { get; set; } = [];
    public ICollection<Reviews> Reviews { get; set; } = [];
}
