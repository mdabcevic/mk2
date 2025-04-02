using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Bartender.Data.Enums;

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

    [Required]
    [Column("label")]
    public string Label { get; set; } = string.Empty;

    [Column("seats")]
    public int Seats { get; set; } = 2;

    [Required]
    [Column("status")]
    public TableStatus Status { get; set; } = TableStatus.empty;

    [Required]
    [Column("qrsalt")]
    public string QrSalt { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    [Column("isdisabled")]
    public bool IsDisabled { get; set; } = false;
}
