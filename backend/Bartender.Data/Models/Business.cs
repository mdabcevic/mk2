using Bartender.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartender.Data.Models;

[Table("businesses")]
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

    [Required]
    [Column("subscriptiontier")] // matches the Postgres column name exactly
    [EnumDataType(typeof(SubscriptionTier))]
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.none;

    public ICollection<Place> Places { get; set; } = [];
}
