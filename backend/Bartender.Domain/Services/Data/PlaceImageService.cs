using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Picture;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Bartender.Domain.Services.Data;

public class PlaceImageService(
    IRepository<Place> placeRepository,
    IRepository<PlaceImage> repository,
    IValidationService validationService,
    IMapper mapper,
    ILogger<PlaceImageService> logger
    ) : IPlaceImageService
{
    public async Task<ServiceResult<List<ImageGroupedDto>>> GetImagesAsync(int placeId, ImageType? pictureType, bool onlyVisible = true)
    {
        var place = await placeRepository.GetByIdAsync(placeId);
        if (place == null)
        {
            logger.LogWarning("Place with id {PlaceId} not found", placeId);
            return ServiceResult<List<ImageGroupedDto>>.Fail($"Place with id {placeId} not found", ErrorType.NotFound);
        }

        var query = repository.QueryIncluding()
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

        logger.LogInformation("Fetched {Count} image groups for place {PlaceId}", pictures.Count, placeId);
        return ServiceResult<List<ImageGroupedDto>>.Ok(pictures);
    }

    public async Task<ServiceResult> AddImageAsync(UpsertImageDto newPicture)
    {
        var validationResult = await ValidatePlaceAndAccessAsync(newPicture.PlaceId);
        if (!validationResult.Success)
            return validationResult;

        var existingPicture = await CheckForExistingImageAsync(newPicture.PlaceId, newPicture.ImageType, newPicture.Url);
        if (existingPicture != null)
        {
            logger.LogWarning("Image already exists in category '{ImageType}' for place {PlaceId}", existingPicture.ImageType, newPicture.PlaceId);
            return ServiceResult.Fail($"Image already exists in category '{existingPicture.ImageType}'", ErrorType.Validation);
        }

        if (newPicture.ImageType == ImageType.banner)
            await HandleBannerChangeAsync(newPicture.PlaceId);

        await repository.AddAsync(mapper.Map<PlaceImage>(newPicture));
        logger.LogInformation("Successfully added image for place {PlaceId}", newPicture.PlaceId);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateImageAsync(int id, UpsertImageDto newPicture)
    {
        var existingPicture = await repository.GetByIdAsync(id);
        if (existingPicture == null)
        {
            logger.LogWarning("Picture with id {ImageId} not found", id);
            return ServiceResult.Fail($"Picture with id {id} not found", ErrorType.NotFound);
        }

        var validationResult = await ValidatePlaceAndAccessAsync(newPicture.PlaceId);
        if (!validationResult.Success)
            return validationResult;

        var existingPictureUrl = await CheckForExistingImageAsync(newPicture.PlaceId, newPicture.ImageType, newPicture.Url, id);
        if (existingPictureUrl != null)
        {
            logger.LogWarning("Image already exists in category '{ImageType}' for place {PlaceId}", newPicture.ImageType, newPicture.PlaceId);
            return ServiceResult.Fail($"Image already exists in category '{existingPicture.ImageType}'", ErrorType.Validation);
        }

        if (newPicture.ImageType == ImageType.banner)
            await HandleBannerChangeAsync(newPicture.PlaceId, id);

        mapper.Map(newPicture, existingPicture);
        await repository.UpdateAsync(existingPicture);
        logger.LogInformation("Successfully updated image with id {ImageId}", id);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteImageAsync(int id)
    {
        var existingPicture = await repository.GetByIdAsync(id);
        if (existingPicture == null)
        {
            logger.LogWarning("Picture with id {ImageId} not found", id);
            return ServiceResult.Fail($"Picture with id {id} not found", ErrorType.NotFound);
        }

        await repository.DeleteAsync(existingPicture);
        logger.LogInformation("Successfully deleted image with id {ImageId}", id);

        return ServiceResult.Ok();
    }

    private async Task<ServiceResult> ValidatePlaceAndAccessAsync(int placeId)
    {
        var existingPlace = await placeRepository.ExistsAsync(p => p.Id == placeId);
        if (!existingPlace)
        {
            logger.LogWarning("Place with id {PlaceId} not found", placeId);
            return ServiceResult.Fail($"Place with id {placeId} not found", ErrorType.NotFound);
        }

        if (!await validationService.VerifyUserPlaceAccess(placeId))
        {
            logger.LogWarning("Cross-business access denied for place {PlaceId}", placeId);
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);
        }

        return ServiceResult.Ok();
    }

    private async Task<PlaceImage?> CheckForExistingImageAsync(int placeId, ImageType imageType, string url, int? id = null)
    {
        return await repository.GetByKeyAsync(pic =>
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
        var existingBannerPicture = await repository.GetByKeyAsync(
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
                await repository.DeleteAsync(existingBannerPicture);
                logger.LogInformation("Deleted existing banner with id {BannerId} due to duplicate gallery picture for place {PlaceId}", existingBannerPicture.Id, placeId);

            }
            else
            {
                existingBannerPicture.ImageType = ImageType.gallery;
                await repository.UpdateAsync(existingBannerPicture);
                logger.LogInformation("Converted previous banner with id {BannerId} to gallery type for place {PlaceId}", existingBannerPicture.Id, placeId);
            }
        }
    }
}
