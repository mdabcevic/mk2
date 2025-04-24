using Bartender.Data.Enums;
using Bartender.Domain.DTO.Place;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Business;

public class BusinessDto
{
    [Required]
    public required string OIB { get; set; }

    [Required]
    public required string Name { get; set; }

    public string? Headquarters { get; set; }

    [EnumDataType(typeof(SubscriptionTier))]
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.none;

    public required List<PlaceDto> Places { get; set; }
}
