using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests.Repositories;

[TestFixture]
public class StaffRepositoryIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task AddAsync_ThenGetByIdAsync_ShouldReturnStaffWithPlaceAndBusiness()
    {
        using var scope = Factory.Services.CreateScope();
        var staffRepo = scope.ServiceProvider.GetRequiredService<IRepository<Staff>>();

        // Arrange 
        var staff = new Staff
        {
            PlaceId = 1,
            OIB = "12345678901",
            Username = "testuserintegration",
            Password = BCrypt.Net.BCrypt.HashPassword("testpassword"),
            FullName = "Test User",
            Role = EmployeeRole.regular
        };

        // Act
        await staffRepo.AddAsync(staff);
        var retrieved = await staffRepo.GetByIdAsync(staff.Id, includeNavigations: true);

        // Assert
        Assert.That(retrieved, Is.Not.Null, "Staff should be retrieved from database");
        Assert.Multiple(() =>
        {
            Assert.That(retrieved!.Username, Is.EqualTo("testuserintegration"), "Username should match");
            Assert.That(retrieved.Place, Is.Not.Null, "Staff should be linked to a Place");
        });
    }

    [Test]
    public async Task ExistsAsync_ShouldReturnTrue_WhenUsernameExists()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var staffRepo = scope.ServiceProvider.GetRequiredService<IRepository<Staff>>();

        var staff = new Staff
        {
            PlaceId = 1,
            OIB = "98765432100",
            Username = "existinguser",
            Password = BCrypt.Net.BCrypt.HashPassword("password"),
            FullName = "Exists User",
            Role = EmployeeRole.regular
        };

        // Act
        await staffRepo.AddAsync(staff);
        var exists = await staffRepo.ExistsAsync(s => s.Username == "existinguser");

        // Assert
        Assert.That(exists, Is.True, "Staff with given username should exist in the database");
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllStaffInDb()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var staffRepo = scope.ServiceProvider.GetRequiredService<IRepository<Staff>>();

        await staffRepo.AddAsync(new Staff
        {
            PlaceId = 1,
            OIB = "99999999999",
            Username = "firstuser",
            Password = "pwd",
            FullName = "First User",
            Role = EmployeeRole.regular
        });

        await staffRepo.AddAsync(new Staff
        {
            PlaceId = 1,
            OIB = "88888888888",
            Username = "seconduser",
            Password = "pwd",
            FullName = "Second User",
            Role = EmployeeRole.manager
        });

        // Act
        var staff = await staffRepo.GetAllAsync();

        // Assert
        Assert.That(staff, Has.Count.GreaterThanOrEqualTo(2), "Should return at least two staff records");
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveStaff()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var staffRepo = scope.ServiceProvider.GetRequiredService<IRepository<Staff>>();

        var staff = new Staff
        {
            PlaceId = 1,
            OIB = "11122233344",
            Username = "deleteuser",
            Password = "pwd",
            FullName = "Delete Me",
            Role = EmployeeRole.regular
        };

        await staffRepo.AddAsync(staff);

        var inserted = await staffRepo.GetByIdAsync(staff.Id);
        Assert.That(inserted, Is.Not.Null, "Staff should be inserted before deletion");

        // Act
        await staffRepo.DeleteAsync(inserted!);
        var deleted = await staffRepo.GetByIdAsync(staff.Id);

        // Assert
        Assert.That(deleted, Is.Null, "Staff should be deleted");
    }

}
