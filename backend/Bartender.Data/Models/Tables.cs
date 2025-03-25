using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

[Table("tables")]
public class Tables
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("place_id")]
    public int PlaceId { get; set; }

    [ForeignKey("PlaceId")]
    public Places? Place { get; set; }

    [Column("seats")]
    public int Seats { get; set; } = 2;

    [Column("status")]
    public string Status { get; set; } = "empty";

    [Column("qrcode")]
    public string? QrCode { get; set; }
}
