using Bartender.Data.Enums;
using Bartender.Domain.DTO.Picture;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface IPlaceImageService
{
    Task<ServiceResult<List<ImageGroupedDto>>> GetImagesAsync(int placeId, ImageType? pictureType = null, bool onlyVisible = true);
    Task<ServiceResult> AddImageAsync(UpsertImageDto newPicture);
    Task<ServiceResult> UpdateImageAsync(int id, UpsertImageDto newPicture);
    Task<ServiceResult> DeleteImageAsync(int id);
}
