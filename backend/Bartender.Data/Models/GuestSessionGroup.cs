using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

public class GuestSessionGroup
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public int TableId { get; set; }

    [ForeignKey(nameof(TableId))]
    public Table Table { get; set; } = null!;

    [Required]
    public string Passphrase { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GuestSession> Sessions { get; set; } = [];
}
