using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartender.Data.Models;

public class GuestSession
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public int TableId { get; set; }

    [ForeignKey(nameof(TableId))]
    public Table? Table { get; set; }

    public Guid? GroupId { get; set; }

    [ForeignKey(nameof(GroupId))]
    public GuestSessionGroup Group { get; set; } = null!;

    [Required]
    public string Token { get; set; } = string.Empty;


    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

    [Required]
    public DateTime ExpiresAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

    [Required]
    public bool IsValid { get; set; } = true;

    public ICollection<Order> Orders { get; set; } = [];
}
