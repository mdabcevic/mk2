using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Bartender.Data.Models;

[Table("guestsessiongroups")]
public class GuestSessionGroup
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("table_id")]
    public int TableId { get; set; }

    [ForeignKey(nameof(TableId))]
    public Table Table { get; set; } = null!;

    [Required]
    [Column("passphrase")]
    public string Passphrase { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GuestSession> Sessions { get; set; } = [];
}
