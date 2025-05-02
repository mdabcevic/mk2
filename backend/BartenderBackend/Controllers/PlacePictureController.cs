using Bartender.Data.Enums;
using Bartender.Domain.DTO.Picture;
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
        return (await placeImageService.GetImagesAsync(placeId, pictureType, onlyVisible)).ToActionResult();
    }

    [Authorize(Roles = "manager, admin, owner")]
    [HttpPost()]
    public async Task<IActionResult> CreateImage([FromBody] UpsertImageDto dto)
    {
        return (await placeImageService.AddImageAsync(dto)).ToActionResult();
    }

    [Authorize(Roles = "manager, admin, owner")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateImage(int id, [FromBody] UpsertImageDto dto)
    {
        return (await placeImageService.UpdateImageAsync(id, dto)).ToActionResult();
    }

    [Authorize(Roles = "manager, admin, owner")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        return (await placeImageService.DeleteImageAsync(id)).ToActionResult();
    }
}
