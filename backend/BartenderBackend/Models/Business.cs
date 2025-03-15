using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BartenderBackend.Models;

[Table("business")]
public class Business
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("oib")]
    public required string OIB { get; set; }

    [Required]
    [Column("name")]
    public required string Name { get; set; }

    [Column("headquarters")]
    public string? Headquarters { get; set; }

    [Column("subscription_tier")]
    public int SubscriptionTier { get; set; } = 1;

    public ICollection<Places> Places { get; set; } = [];
}
