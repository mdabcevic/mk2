using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BartenderBackend.Models;

[Table("staff")]
public class Staff
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Places")]
    [Column("place_id")]
    public int PlaceId { get; set; }

    [ForeignKey("PlaceId")]
    public Places? Place { get; set; }

    [Required, MaxLength(11)]
    [Column("oib")]
    public required string OIB { get; set; }

    [Required]
    [Column("username")]
    public required string Username { get; set; }

    [Required]
    [Column("password")]
    public required string Password { get; set; }

    [Required]
    [Column("fullname")]
    public required string FullName { get; set; }

    [Column("role")]
    public string Role { get; set; } = "bartender";
}
