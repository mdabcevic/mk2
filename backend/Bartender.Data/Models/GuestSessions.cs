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

    [Column("group_id")]
    public Guid GroupId { get; set; }

    [ForeignKey(nameof(GroupId))]
    public GuestSessionGroup Group { get; set; } = null!;

    [Required]
    [Column("token")]
    public string Token { get; set; } = string.Empty;


    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Required]
    [Column("isvalid")]
    public bool IsValid { get; set; } = true;

    public ICollection<Orders> Orders { get; set; } = [];
}
