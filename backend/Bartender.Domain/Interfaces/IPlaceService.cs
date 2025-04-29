using Bartender.Data.Enums;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Picture;
using Bartender.Domain.DTO.Place;

namespace Bartender.Domain.Interfaces;

public interface IPlaceService
{
    Task<ServiceResult<PlaceWithMenuDto>> GetByIdAsync(int id, bool includeNavigations = true);
    Task<ServiceResult<List<PlaceDto>>> GetAllAsync();
    Task<ServiceResult> AddAsync(InsertPlaceDto dto);
    Task<ServiceResult> UpdateAsync(int id, UpdatePlaceDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> NotifyStaffAsync(string salt);
    Task<ServiceResult<List<ImageGroupedDto>>> GetImagesAsync(int placeId, ImageType? pictureType, bool onlyVisible = true);
    Task<ServiceResult> AddImageAsync(UpsertImageDto newPicture);
    Task<ServiceResult> UpdateImageAsync(int id, UpsertImageDto newPicture);
    Task<ServiceResult> DeleteImageAsync(int id);
}
