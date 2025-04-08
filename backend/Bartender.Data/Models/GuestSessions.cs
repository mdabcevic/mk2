using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartender.Data.Models;

[Table("guestsessions")]
public class GuestSession
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("table_id")]
    public int TableId { get; set; }

    [ForeignKey(nameof(TableId))]
    public Tables? Table { get; set; }

    [Required]
    [Column("token")]
    public string Token { get; set; } = string.Empty;

    [Column("passphrase")]
    public string? Passphrase { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public ICollection<Orders> Orders { get; set; } = [];
}
