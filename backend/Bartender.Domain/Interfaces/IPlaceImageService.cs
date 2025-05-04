using Bartender.Data.Enums;
using Bartender.Domain.DTO.Picture;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface IPlaceImageService
{
    Task<List<ImageGroupedDto>> GetImagesAsync(int placeId, ImageType? pictureType = null, bool onlyVisible = true);
    Task AddImageAsync(UpsertImageDto newPicture);
    Task UpdateImageAsync(int id, UpsertImageDto newPicture);
    Task DeleteImageAsync(int id);
}
