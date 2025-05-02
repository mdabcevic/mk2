
using Bartender.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartender.Data.Models;

public class PlaceImage : BaseEntity
{
    [Required]
    public int PlaceId { get; set; }

    [ForeignKey(nameof(PlaceId))]
    public Place? Place { get; set; }

    [Required]
    public required string Url { get; set; }

    [Required]
    [EnumDataType(typeof(ImageType))]
    public ImageType ImageType { get; set; }

    public bool IsVisible { get; set; } = true;
}
