using AutoMapper;
using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Picture;
using Bartender.Domain.DTO.Place;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services.Data;

public class PlaceService(
    IRepository<Place> repository,
    IRepository<PlaceImage> pictureRepository,
    ITableRepository tableRepository,
    ILogger<PlaceService> logger,
    ICurrentUserContext currentUser,
    INotificationService notificationService,
    IValidationService validationService,
    IMapper mapper
    )
    : IPlaceService
{
    public async Task<ServiceResult> AddAsync(InsertPlaceDto dto)
    {
        if (!await IsSameBusinessAsync(dto.BusinessId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        var entity = mapper.Map<Place>(dto);
        await repository.AddAsync(entity);
        logger.LogInformation("Place created: {Address}, BusinessId: {BusinessId}", dto.Address, dto.BusinessId);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var place = await repository.GetByIdAsync(id);
        if (place == null)
            return ServiceResult.Fail($"Place with ID {id} not found.", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(place.BusinessId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        await repository.DeleteAsync(place);
        logger.LogInformation("Place deleted with ID: {PlaceId}", id);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<List<PlaceDto>>> GetAllAsync()
    {
        var placesWithMenus = await repository.QueryIncluding(
            p => p.Business,
            p => p.City,
            p => p.Images
        ).ToListAsync();

        var list = placesWithMenus.Select(p =>
        {
            var dto = mapper.Map<PlaceDto>(p);
            dto.Banner = p.Images?
                .Where(i => i.ImageType == ImageType.banner && i.IsVisible)
                .Select(i => i.Url)
                .FirstOrDefault();
            return dto;
        }).ToList();

        return ServiceResult<List<PlaceDto>>.Ok(list);
    }

    public async Task<ServiceResult<PlaceWithMenuDto>> GetByIdAsync(int id, bool includeNavigations = true)
    {
        //TODO: move query to dedicated repository
        var place = await repository.Query()
        .Include(p => p.Business)
        .Include(p => p.City)
        .Include(p => p.Tables)
        .Include(p => p.Images)
        .Include(p => p.MenuItems)!
            .ThenInclude(mi => mi.Product)
        .FirstOrDefaultAsync(p => p.Id == id);

        if (place == null)
            return ServiceResult<PlaceWithMenuDto>.Fail($"Place with ID {id} not found.", ErrorType.NotFound);

        var groupedImages = place.Images?
            .Where(i => i.IsVisible)
            .GroupBy(i => i.ImageType)
            .Select( g => new ImageGroupedDto
            {
                ImageType = g.Key,
                Urls = g.Select(i => i.Url).ToList()
            })
            .ToList();

        var dto = mapper.Map<PlaceWithMenuDto>(place);

        if (groupedImages != null)
            dto.Images = groupedImages;

        return ServiceResult<PlaceWithMenuDto>.Ok(dto);
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdatePlaceDto dto)
    {
        var place = await repository.GetByIdAsync(id);
        if (place == null)
            return ServiceResult.Fail($"Place with ID {id} not found.", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(place.BusinessId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        mapper.Map(dto, place);
        await repository.UpdateAsync(place);
        logger.LogInformation("Place updated with ID: {PlaceId}", place.Id);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> NotifyStaffAsync(string salt)
    {
        var table = await tableRepository.GetBySaltAsync(salt);

        if (table is null)
        {
            logger.LogWarning("NotifyStaff failed: Table does not exist.");
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }

        await notificationService.AddNotificationAsync(table, 
            NotificationFactory.ForTableStatus(table, $"Waiter requested at table {table.Label}.", NotificationType.StaffNeeded));

        logger.LogInformation("Staff notified for table {Label}", table.Label);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<List<ImageGroupedDto>>> GetImagesAsync(int placeId, ImageType? pictureType, bool onlyVisible = true)
    {
        var place = await repository.GetByIdAsync(placeId);
        if (place == null)
            return ServiceResult<List<ImageGroupedDto>>.Fail($"Place with id {placeId} not found", ErrorType.NotFound);

        var query = pictureRepository.QueryIncluding()
        .Where(pic => pic.PlaceId == placeId);

        if (pictureType != null)
            query = query.Where(pic => pic.ImageType == pictureType.Value);

        if (onlyVisible)
            query = query.Where(pic => pic.IsVisible);

       var pictures = await query
            .GroupBy(pic => pic.ImageType)
            .Select(g => new ImageGroupedDto
            {
                ImageType = g.Key,
                Images = onlyVisible ? null : g.Select(pic => new ImageDto
                {
                    Id = pic.Id,
                    Url = pic.Url,
                    IsVisible = onlyVisible ? null : pic.IsVisible
                }).ToList(),
                Urls = onlyVisible ? g.Select(pic => pic.Url).ToList() : null
            })
            .ToListAsync();

        return ServiceResult<List<ImageGroupedDto>>.Ok(pictures);
    }

    public async Task<ServiceResult> AddImageAsync(UpsertImageDto newPicture)
    {
        var validationResult = await ValidatePlaceAndAccessAsync(newPicture.PlaceId);
        if (!validationResult.Success)
            return validationResult;

        var existingPicture = await CheckForExistingImageAsync(newPicture.PlaceId, newPicture.ImageType, newPicture.Url);
        if (existingPicture != null)
            return ServiceResult.Fail($"Image already exists in category '{existingPicture.ImageType}'", ErrorType.Validation);

        if (newPicture.ImageType == ImageType.banner)
            await HandleBannerChangeAsync(newPicture.PlaceId);
        
        await pictureRepository.AddAsync(mapper.Map<PlaceImage>(newPicture));
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateImageAsync(int id, UpsertImageDto newPicture)
    {
        var existingPicture = await pictureRepository.GetByIdAsync(id);
        if(existingPicture == null)
            return ServiceResult.Fail($"Picture with id {id} not found", ErrorType.NotFound);

        var validationResult = await ValidatePlaceAndAccessAsync(newPicture.PlaceId);
        if (!validationResult.Success)
            return validationResult;

        var existingPictureUrl = await CheckForExistingImageAsync(newPicture.PlaceId, newPicture.ImageType, newPicture.Url, id);
        if (existingPictureUrl != null)
            return ServiceResult.Fail($"Image already exists in category '{existingPicture.ImageType}'", ErrorType.Validation);

        if (newPicture.ImageType == ImageType.banner)
            await HandleBannerChangeAsync(newPicture.PlaceId, id);

        mapper.Map(newPicture, existingPicture);
        await pictureRepository.UpdateAsync(existingPicture);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteImageAsync(int id)
    {
        var existingPicture = await pictureRepository.GetByIdAsync(id);
        if (existingPicture == null)
            return ServiceResult.Fail($"Picture with id {id} not found", ErrorType.NotFound);

        await pictureRepository.DeleteAsync(existingPicture);
        return ServiceResult.Ok();
    }

    private async Task<ServiceResult> ValidatePlaceAndAccessAsync(int placeId)
    {
        var existingPlace = await repository.ExistsAsync(p => p.Id == placeId);
        if (!existingPlace)
            return ServiceResult.Fail($"Place with id {placeId} not found", ErrorType.NotFound);

        if (!await validationService.VerifyUserPlaceAccess(placeId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        return ServiceResult.Ok();
    }

    private async Task<PlaceImage?> CheckForExistingImageAsync(int placeId, ImageType imageType, string url, int? id = null)
    {
        return await pictureRepository.GetByKeyAsync(pic =>
            pic.PlaceId == placeId &&
            pic.ImageType == imageType &&
            pic.Url == url &&
            (id == null || pic.Id != id));
    }

    /// <summary>
    /// 
    /// Ensures a place has only one banner image.
    /// If another banner exists:
    /// - Deletes it if a matching gallery image exists,
    /// - Otherwise, converts it to a gallery image.
    /// 
    /// </summary>
    /// <param name="placeId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task HandleBannerChangeAsync(int placeId, int? id = null)
    {
        var existingBannerPicture = await pictureRepository.GetByKeyAsync(
            pic => pic.PlaceId == placeId && pic.ImageType == ImageType.banner && (id == null || pic.Id != id));

        if (existingBannerPicture != null)
        {
            var duplicateGalleryPicture = await CheckForExistingImageAsync(
                existingBannerPicture.PlaceId,
                ImageType.gallery,
                existingBannerPicture.Url,
                existingBannerPicture.Id);

            if (duplicateGalleryPicture != null)
            {
                await pictureRepository.DeleteAsync(existingBannerPicture);
            }
            else
            {
                existingBannerPicture.ImageType = ImageType.gallery;
                await pictureRepository.UpdateAsync(existingBannerPicture);
            }
        }
    }

    //TODO: move to validation
    private async Task<bool> IsSameBusinessAsync(int targetPlaceId)
    {
        var user = await currentUser.GetCurrentUserAsync();
        return targetPlaceId == user.PlaceId;
    }
}
