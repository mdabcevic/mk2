using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Bartender.Data.Enums;

namespace Bartender.Data.Models;

public class Table : BaseEntity
{
    [Required]
    public int PlaceId { get; set; }

    [ForeignKey(nameof(PlaceId))]
    public Place? Place { get; set; }

    [Required]
    public string Label { get; set; } = string.Empty;

    public int Seats { get; set; } = 2;

    [Required]
    public int? Width { get; set; }

    [Required]
    public int? Height { get; set; }

    [Required]
    [Column(TypeName = "decimal(6,2)")]
    public decimal? X { get; set; }

    [Required]
    [Column(TypeName = "decimal(6,2)")]
    public decimal? Y { get; set; }

    [Required]
    public TableStatus Status { get; set; } = TableStatus.empty;

    [Required]
    public string QrSalt { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    public bool IsDisabled { get; set; } = false;

    public ICollection<Order>? Orders { get; set; } = [];
}
