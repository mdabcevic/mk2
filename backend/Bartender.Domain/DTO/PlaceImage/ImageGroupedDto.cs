﻿
using Bartender.Data.Enums;
using System.Text.Json.Serialization;

namespace Bartender.Domain.DTO.Picture;

public class ImageGroupedDto
{
    public required ImageType ImageType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ImageDto>? Images { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Urls { get; set; }
}
