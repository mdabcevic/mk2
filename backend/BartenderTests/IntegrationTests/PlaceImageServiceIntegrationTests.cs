using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Picture;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests;

[TestFixture]
public class PlaceImageServiceIntegrationTests : IntegrationTestBase
{

    private IPlaceImageService _service = null!;
    private IRepository<Place> _placeRepo = null!;
    private IRepository<PlaceImage> _imageRepo = null!;
    private MockCurrentUser _mockUser = null!;

    [SetUp]
    public void SetUp()
    {
        var scope = Factory.Services.CreateScope();
        _service = scope.ServiceProvider.GetRequiredService<IPlaceImageService>();
        _placeRepo = scope.ServiceProvider.GetRequiredService<IRepository<Place>>();
        _imageRepo = scope.ServiceProvider.GetRequiredService<IRepository<PlaceImage>>();
        _mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
    }

    // ---------------------------
    // GET
    // ---------------------------

    [Test]
    public async Task GetImagesAsync_ShouldReturnVisibleImagesGroupedByType()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1);
        _mockUser.Override(staff);

        await _imageRepo.AddMultipleAsync(new[]
        {
        new PlaceImage { PlaceId = 1, ImageType = ImageType.gallery, Url = "http://1.jpg", IsVisible = true },
        new PlaceImage { PlaceId = 1, ImageType = ImageType.gallery, Url = "http://2.jpg", IsVisible = true },
        new PlaceImage { PlaceId = 1, ImageType = ImageType.banner, Url = "http://banner.jpg", IsVisible = true },
        new PlaceImage { PlaceId = 1, ImageType = ImageType.gallery, Url = "http://hidden.jpg", IsVisible = false },
    });

        var result = await _service.GetImagesAsync(placeId: 1, pictureType: null, onlyVisible: true);

        Assert.That(result, Has.Count.GreaterThanOrEqualTo(2)); // gallery and banner
        Assert.That(result.SelectMany(g => g.Urls!), Does.Not.Contain("http://hidden.jpg"));
    }

    [Test]
    public async Task GetImagesAsync_ShouldIncludeInvisible_WhenOnlyVisibleIsFalse()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1);
        _mockUser.Override(staff);

        await _imageRepo.AddAsync(new PlaceImage
        {
            PlaceId = 1,
            ImageType = ImageType.gallery,
            Url = "http://hidden.jpg",
            IsVisible = false
        });

        var result = await _service.GetImagesAsync(placeId: 1, pictureType: null, onlyVisible: false);

        var allImages = result.SelectMany(g => g.Images ?? []).ToList();
        Assert.That(allImages, Has.Some.Matches<ImageDto>(img => img.Url == "http://hidden.jpg"));
    }

    [Test]
    public void GetImagesAsync_ShouldFail_WhenPlaceNotFound()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1);
        _mockUser.Override(staff);

        var ex = Assert.ThrowsAsync<PlaceNotFoundException>(() => _service.GetImagesAsync(9999));
        Assert.That(ex, Is.Not.Null);
    }


    // ---------------------------
    // AddImageAsync
    // ---------------------------

    [Test]
    public async Task AddImageAsync_ShouldAddImage_WhenValid()
    {
        // Arrange
        var place = new Place { BusinessId = 1, CityId = 1, Address = "Main 1" };
        await _placeRepo.AddAsync(place);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: place.Id, businessid: place.BusinessId, role: EmployeeRole.owner));

        var dto = new UpsertImageDto
        {
            PlaceId = place.Id,
            ImageType = ImageType.gallery,
            Url = "https://cdn.test.com/images/valid.jpg",
        };

        // Act
        await _service.AddImageAsync(dto);

        // Assert
        var saved = await _imageRepo.GetByKeyAsync(p => p.PlaceId == place.Id && p.Url == dto.Url);
        Assert.That(saved, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(saved!.ImageType, Is.EqualTo(dto.ImageType));
            Assert.That(saved.Url, Is.EqualTo(dto.Url));
        });
    }

    [Test]
    public void AddImageAsync_ShouldFail_WhenPlaceDoesNotExist()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1, role: EmployeeRole.owner);
        _mockUser.Override(staff);

        var dto = new UpsertImageDto
        {
            PlaceId = 999,
            ImageType = ImageType.gallery,
            Url = "http://example.com/new.jpg"
        };

        var ex = Assert.ThrowsAsync<PlaceNotFoundException>(() => _service.AddImageAsync(dto));
        Assert.That(ex!.Message, Does.Contain("Place"));
    }

    [Test]
    public void AddImageAsync_ShouldFail_WhenUserUnauthorized()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 2, businessid: 1, role: EmployeeRole.regular);
        _mockUser.Override(staff);

        var dto = new UpsertImageDto
        {
            PlaceId = 1,
            ImageType = ImageType.gallery,
            Url = "http://example.com/new.jpg"
        };

        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(() => _service.AddImageAsync(dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task AddImageAsync_ShouldFail_WhenDuplicateExists()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1, role: EmployeeRole.manager);
        _mockUser.Override(staff);

        var existing = new PlaceImage { PlaceId = 1, ImageType = ImageType.gallery, Url = "http://example.com/dupe.jpg" };
        await _imageRepo.AddAsync(existing);

        var dto = new UpsertImageDto
        {
            PlaceId = 1,
            ImageType = ImageType.gallery,
            Url = "http://example.com/dupe.jpg"
        };

        var ex = Assert.ThrowsAsync<ConflictException>(() => _service.AddImageAsync(dto));
        Assert.That(ex!.Message, Does.Contain("already exists"));
    }

    //[Test]
    //public async Task AddImageAsync_ShouldReplaceBanner_WhenDuplicateGalleryExists()
    //{
    //    var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1, role: EmployeeRole.manager);
    //    _mockUser.Override(staff);

    //    await _imageRepo.AddAsync(new PlaceImage { PlaceId = 1, ImageType = ImageType.gallery, Url = "http://img.jpg" });
    //    await _imageRepo.AddAsync(new PlaceImage { PlaceId = 1, ImageType = ImageType.banner, Url = "http://img.jpg" });

    //    var dto = new UpsertImageDto
    //    {
    //        PlaceId = 1,
    //        ImageType = ImageType.banner,
    //        Url = "http://img.jpg"
    //    };

    //    await _service.AddImageAsync(dto);

    //    var banners = await _imageRepo.GetFilteredAsync(filterBy: p => p.PlaceId == 1 && p.ImageType == ImageType.banner);
    //    Assert.That(banners[0].Url, Is.EqualTo("http://img.jpg")); // the newly added one
    //}

    //[Test]
    //public async Task AddImageAsync_ShouldConvertBanner_WhenNoDuplicateGalleryExists()
    //{
    //    var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1, role: EmployeeRole.manager);
    //    _mockUser.Override(staff);

    //    var oldBanner = new PlaceImage { PlaceId = 1, ImageType = ImageType.banner, Url = "http://old-banner.jpg" };
    //    await _imageRepo.AddAsync(oldBanner);

    //    var dto = new UpsertImageDto
    //    {
    //        PlaceId = 1,
    //        ImageType = ImageType.banner,
    //        Url = "http://new-banner.jpg"
    //    };

    //    await _service.AddImageAsync(dto);

    //    var updatedOld = await _imageRepo.GetByIdAsync(oldBanner.Id);
    //    Assert.That(updatedOld!.ImageType, Is.EqualTo(ImageType.gallery));
    //}



    // ---------------------------
    // Update
    // ---------------------------

    [Test]
    public async Task UpdateImageAsync_ShouldUpdate_WhenValid()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1);
        _mockUser.Override(staff);

        var image = new PlaceImage { PlaceId = 1, ImageType = ImageType.gallery, Url = "http://old.jpg" };
        await _imageRepo.AddAsync(image);

        var dto = new UpsertImageDto
        {
            PlaceId = 1,
            ImageType = ImageType.gallery,
            Url = "http://new.jpg"
        };

        await _service.UpdateImageAsync(image.Id, dto);

        var updated = await _imageRepo.GetByIdAsync(image.Id);
        Assert.That(updated!.Url, Is.EqualTo("http://new.jpg"));
    }

    [Test]
    public async Task UpdateImageAsync_ShouldReplaceOldBanner_WhenDuplicateGalleryExists()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1);
        _mockUser.Override(staff);

        var gallery = new PlaceImage { PlaceId = 1, ImageType = ImageType.gallery, Url = "http://shared.jpg" };
        var toUpdate = new PlaceImage { PlaceId = 1, ImageType = ImageType.banner, Url = "http://old-banner.jpg" };
        await _imageRepo.AddAsync(gallery);
        await _imageRepo.AddAsync(toUpdate);

        var dto = new UpsertImageDto
        {
            PlaceId = 1,
            ImageType = ImageType.banner,
            Url = "http://shared.jpg"
        };

        await _service.UpdateImageAsync(toUpdate.Id, dto);

        var updated = await _imageRepo.GetByIdAsync(toUpdate.Id);
        Assert.That(updated!.Url, Is.EqualTo("http://shared.jpg"));
    }

    [Test]
    public void UpdateImageAsync_ShouldFail_WhenImageNotFound()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1);
        _mockUser.Override(staff);

        var dto = new UpsertImageDto
        {
            PlaceId = 1,
            ImageType = ImageType.gallery,
            Url = "http://new.jpg"
        };

        var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateImageAsync(9999, dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task UpdateImageAsync_ShouldFail_WhenUnauthorized()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1);
        var outsider = TestDataFactory.CreateValidStaff(placeid: 2, businessid: 1);
        await _imageRepo.AddAsync(new PlaceImage { PlaceId = 1, ImageType = ImageType.gallery, Url = "http://update.jpg" });

        _mockUser.Override(outsider); // no access to PlaceId = 1

        var dto = new UpsertImageDto
        {
            PlaceId = 1,
            ImageType = ImageType.gallery,
            Url = "http://new.jpg"
        };

        var image = await _imageRepo.GetByKeyAsync(p => p.PlaceId == 1 && p.Url == "http://update.jpg");

        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(() => _service.UpdateImageAsync(image!.Id, dto));
        Assert.That(ex, Is.Not.Null);
    }


    // ---------------------------
    // DELETE
    // ---------------------------

    [Test]
    public async Task DeleteImageAsync_ShouldDelete_WhenAuthorized()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1);
        _mockUser.Override(staff);

        var image = new PlaceImage { PlaceId = 1, ImageType = ImageType.gallery, Url = "http://delete.jpg" };
        await _imageRepo.AddAsync(image);

        await _service.DeleteImageAsync(image.Id);

        var deleted = await _imageRepo.GetByIdAsync(image.Id);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public void DeleteImageAsync_ShouldFail_WhenImageNotFound()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1);
        _mockUser.Override(staff);

        var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteImageAsync(9999));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task DeleteImageAsync_ShouldFail_WhenUnauthorized()
    {
        var staff = TestDataFactory.CreateValidStaff(placeid: 2, businessid: 1); // has no access to place 1
        _mockUser.Override(staff);

        var image = new PlaceImage { PlaceId = 1, ImageType = ImageType.gallery, Url = "http://noaccess.jpg" };
        await _imageRepo.AddAsync(image);

        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(() => _service.DeleteImageAsync(image.Id));
        Assert.That(ex, Is.Not.Null);
    }

}
