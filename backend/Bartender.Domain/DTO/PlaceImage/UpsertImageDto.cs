using Bartender.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.PlaceImage;

public class UpsertImageDto
{
    [Required]
    public required string Url { get; set; }
    [Required]
    public required int PlaceId { get; set; }
    [Required]
    public required ImageType ImageType { get; set; }
    public bool isVisible { get; set; } = true;
}
