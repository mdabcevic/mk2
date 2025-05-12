using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Place;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests;

[TestFixture]
public class PlaceServiceIntegrationTests : IntegrationTestBase
{
    private IPlaceService _service = null!;
    private IRepository<Place> _placeRepo = null!;
    private IRepository<City> _cityRepo = null!;
    private IRepository<Business> _businessRepo = null!;
    private IRepository<Table> _tableRepo = null!;
    private MockCurrentUser _mockUser = null!;

    [SetUp]
    public void SetUp()
    {
        var scope = Factory.Services.CreateScope();
        _service = scope.ServiceProvider.GetRequiredService<IPlaceService>();
        _placeRepo = scope.ServiceProvider.GetRequiredService<IRepository<Place>>();
        _cityRepo = scope.ServiceProvider.GetRequiredService<IRepository<City>>();
        _businessRepo = scope.ServiceProvider.GetRequiredService<IRepository<Business>>();
        _tableRepo = scope.ServiceProvider.GetRequiredService<IRepository<Table>>();
        _mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
    }

    [Test]
    public async Task AddAsync_ShouldAddPlace_WhenAuthorized()
    {
        _mockUser.Override(new Staff
        {
            Id = 1,
            Username = "manager",
            FullName = "Full Name",
            OIB = "00000000000",
            Role = EmployeeRole.admin,
            Password = "x",
            Place = new Place { BusinessId = 1, Address = "Address" }
        });

        var dto = new InsertPlaceDto
        {
            Address = "New Place",
            BusinessId = 1,
            CityId = 1,
            Description = "Test",
            OpensAt = "08:00",
            ClosesAt = "22:00"
        };

        await _service.AddAsync(dto);

        var saved = await _placeRepo.GetByKeyAsync(p => p.Address == "New Place");
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.BusinessId, Is.EqualTo(1));
    }

    [Test]
    public async Task AddAsync_ShouldFail_WhenBusinessMismatch()
    {
        var city = new City { Name = "Mismatch City" };
        await _cityRepo.AddAsync(city);

        var authorizedBusiness = new Business { Name = "Real", OIB = "88888888888" };
        var otherBusiness = new Business { Name = "Other", OIB = "12345678901" };
        await _businessRepo.AddAsync(authorizedBusiness);
        await _businessRepo.AddAsync(otherBusiness);

        _mockUser.Override(new Staff
        {
            Id = 2,
            Username = "unauthorized",
            FullName = "Name",
            OIB = "11111111111",
            Role = EmployeeRole.manager,
            Password = "x",
            Place = new Place { BusinessId = authorizedBusiness.Id, Address = "Address" }
        });

        var dto = new InsertPlaceDto
        {
            Address = "Mismatch",
            BusinessId = otherBusiness.Id,
            CityId = city.Id,
            Description = "Wrong Biz",
            OpensAt = "07:00",
            ClosesAt = "23:00"
        };

        var ex = Assert.ThrowsAsync<UnauthorizedBusinessAccessException>(() => _service.AddAsync(dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldDelete_WhenAuthorized()
    {
        var business = new Business { Name = "Delete Biz", OIB = "77777777777" };
        await _businessRepo.AddAsync(business);

        var city = new City { Name = "City" };
        await _cityRepo.AddAsync(city);

        var place = new Place { Address = "To Delete", BusinessId = business.Id, CityId = city.Id };
        await _placeRepo.AddAsync(place);

        _mockUser.Override(new Staff
        {
            Id = 3,
            Username = "deleter",
            FullName = "Delete User",
            OIB = "22222222222",
            Role = EmployeeRole.owner,
            Password = "x",
            Place = place
        });

        await _service.DeleteAsync(place.Id);
        var exists = await _placeRepo.GetByIdAsync(place.Id);
        Assert.That(exists, Is.Null);
    }

    [Test]
    public void DeleteAsync_ShouldFail_WhenNotFound()
    {
        var ex = Assert.ThrowsAsync<PlaceNotFoundException>(() => _service.DeleteAsync(9999));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnPlaces_WithBannerImage()
    {
        var city = new City { Name = "City" };
        var business = new Business { Name = "Biz", OIB = "11122233344" };
        await _cityRepo.AddAsync(city);
        await _businessRepo.AddAsync(business);

        var place = new Place
        {
            Address = "WithBanner",
            BusinessId = business.Id,
            CityId = city.Id,
            Images =
            [
                new PlaceImage { ImageType = ImageType.banner, Url = "banner.jpg", IsVisible = true }
            ]
        };

        await _placeRepo.AddAsync(place);

        var list = await _service.GetAllAsync();
        var dto = list.FirstOrDefault(p => p.Address == "WithBanner");

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Banner, Is.EqualTo("banner.jpg"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnPlaceDetails()
    {
        var city = new City { Name = "Zagreb" };
        await _cityRepo.AddAsync(city);

        var business = new Business { Name = "Details Inc", OIB = "66666666666" };
        await _businessRepo.AddAsync(business);

        var place = new Place
        {
            Address = "Details Place",
            CityId = city.Id,
            BusinessId = business.Id
        };

        await _placeRepo.AddAsync(place);

        var dto = await _service.GetByIdAsync(place.Id);
        Assert.That(dto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dto.Address, Is.EqualTo("Details Place"));
            Assert.That(dto.Images, Is.Not.Null);
        });
    }

    [Test]
    public void GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var ex = Assert.ThrowsAsync<PlaceNotFoundException>(() => _service.GetByIdAsync(9999));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdate_WhenAuthorized()
    {
        var business = new Business { Name = "Updater", OIB = "55544433322" };
        await _businessRepo.AddAsync(business);

        var city = new City { Name = "City" };
        await _cityRepo.AddAsync(city);

        var place = new Place
        {
            Address = "Before",
            CityId = city.Id,
            BusinessId = business.Id
        };
        await _placeRepo.AddAsync(place);

        _mockUser.Override(new Staff
        {
            Id = 9,
            Username = "editor",
            FullName = "Full",
            OIB = "00000000000",
            Role = EmployeeRole.manager,
            Password = "x",
            Place = place
        });

        var dto = new UpdatePlaceDto
        {
            Address = "After",
            Description = "Updated"
        };

        await _service.UpdateAsync(place.Id, dto);
        var updated = await _placeRepo.GetByIdAsync(place.Id);
        Assert.That(updated!.Address, Is.EqualTo("After"));
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenUnauthorized()
    {
        var business1 = new Business { Name = "Owner", OIB = "22233344455" };
        var business2 = new Business { Name = "Other", OIB = "55566677788" };
        await _businessRepo.AddAsync(business1);
        await _businessRepo.AddAsync(business2);

        var city = new City { Name = "City" };
        await _cityRepo.AddAsync(city);

        var place = new Place { Address = "Protected", BusinessId = business2.Id, CityId = city.Id };
        await _placeRepo.AddAsync(place);

        _mockUser.Override(new Staff
        {
            Id = 10,
            Username = "outsider",
            FullName = "Not Allowed",
            OIB = "11111111111",
            Role = EmployeeRole.manager,
            Password = "x",
            Place = new Place { BusinessId = business1.Id, Address = "Address" }
        });

        var dto = new UpdatePlaceDto { Address = "Attempted" };

        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(() => _service.UpdateAsync(place.Id, dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task NotifyStaffAsync_ShouldNotify_WhenSaltValid()
    {
        var business = new Business { Name = "Notify Biz", OIB = "33322211100" };
        await _businessRepo.AddAsync(business);

        var table = new Table
        {
            Label = "A1",
            PlaceId = 1,
            QrSalt = "validsalt",
            Status = TableStatus.occupied,
            Width = 100,
            Height = 100,
            X = 0,
            Y = 0
        };
        await _tableRepo.AddAsync(table);

        Assert.DoesNotThrowAsync(() => _service.NotifyStaffAsync("validsalt"));
    }

    [Test]
    public void NotifyStaffAsync_ShouldFail_WhenTableMissing()
    {
        var ex = Assert.ThrowsAsync<TableNotFoundException>(() => _service.NotifyStaffAsync("invalidsalt"));
        Assert.That(ex, Is.Not.Null);
    }
}
