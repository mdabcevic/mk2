using Bartender.Data.Enums;
using Bartender.Domain.DTO.PlaceImage;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/images")]
[ApiController]
public class PlacePictureController(IPlaceImageService placeImageService) : ControllerBase
{
    [HttpGet("{placeId}")]
    public async Task<IActionResult> GetImagesByPlace(int placeId, [FromQuery] ImageType? pictureType, [FromQuery] bool onlyVisible = true)
    {
        var result = await placeImageService.GetImagesAsync(placeId, pictureType, onlyVisible);
        return Ok(result);
    }

    [Authorize(Roles = "manager, admin, owner")]
    [HttpPost()]
    public async Task<IActionResult> CreateImage([FromBody] UpsertImageDto dto)
    {
        await placeImageService.AddImageAsync(dto);
        return NoContent();
    }

    [Authorize(Roles = "manager, admin, owner")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateImage(int id, [FromBody] UpsertImageDto dto)
    {
        await placeImageService.UpdateImageAsync(id, dto);
        return NoContent();
    }

    [Authorize(Roles = "manager, admin, owner")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        await placeImageService.DeleteImageAsync(id);
        return NoContent();
    }
}
