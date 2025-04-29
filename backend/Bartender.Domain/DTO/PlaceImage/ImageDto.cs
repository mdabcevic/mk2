using Bartender.Data.Enums;
using System.Text.Json.Serialization;

namespace Bartender.Domain.DTO.Picture;

public class ImageDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id {  get; set; }

    public required string Url { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImageType? ImageType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsVisible { get; set; }
}
