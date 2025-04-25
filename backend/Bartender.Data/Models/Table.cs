using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Bartender.Data.Enums;

namespace Bartender.Data.Models;

[Table("tables")]
public class Table
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("place_id")]
    public int PlaceId { get; set; }

    [ForeignKey("PlaceId")]
    public Place? Place { get; set; }

    [Required]
    [Column("label")]
    public string Label { get; set; } = string.Empty;

    [Column("seats")]
    public int Seats { get; set; } = 2;

    [Required]
    [Column("width")]
    public int? Width { get; set; }

    [Required]
    [Column("height")]
    public int? Height { get; set; }

    [Required]
    [Column("xcoordinate", TypeName = "decimal(6,2)")]
    public decimal? X { get; set; }

    [Required]
    [Column("ycoordinate", TypeName = "decimal(6,2)")]
    public decimal? Y { get; set; }

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
