using Bartender.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Data.Models;

public class Business : BaseEntity
{
    
    [Required]
    public required string OIB { get; set; }

    [Required]
    public required string Name { get; set; }

    public string? Headquarters { get; set; }

    [Required]
    [EnumDataType(typeof(SubscriptionTier))]
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.none;

    public ICollection<Place> Places { get; set; } = [];
}
