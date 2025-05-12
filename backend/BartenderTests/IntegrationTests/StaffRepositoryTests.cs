using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests;

[TestFixture]
public class StaffRepositoryTests : IntegrationTestBase
{
    [Test]
    public async Task AddAsync_ThenGetByIdAsync_ShouldReturnStaffWithPlaceAndBusiness()
    {
        using var scope = Factory.Services.CreateScope();
        var staffRepo = scope.ServiceProvider.GetRequiredService<IRepository<Staff>>();

        // Arrange 
        var staff = new Staff
        {
            PlaceId = 1, // relies on seeded data
            OIB = "12345678901",
            Username = "testuserintegration",
            Password = BCrypt.Net.BCrypt.HashPassword("testpassword"),
            FullName = "Test User",
            Role = EmployeeRole.regular
        };
        await staffRepo.AddAsync(staff);

        // Act
        var retrieved = await staffRepo.GetByIdAsync(staff.Id, includeNavigations: true);

        // Assert
        Assert.That(retrieved, Is.Not.Null, "Staff should be retrieved from database");
        Assert.Multiple(() =>
        {
            Assert.That(retrieved!.Username, Is.EqualTo("testuserintegration"), "Username should match");
            Assert.That(retrieved.Place, Is.Not.Null, "Staff should be linked to a Place");
        });
    }
}
