using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests;

[TestFixture]
public class StaffServiceIntegrationTests : IntegrationTestBase
{
    private IStaffService _service = null!;
    private IRepository<Staff> _staffRepo = null!;

    [SetUp]
    public void SetUp()
    {
        var scope = Factory.Services.CreateScope();
        _service = scope.ServiceProvider.GetRequiredService<IStaffService>();
        _staffRepo = scope.ServiceProvider.GetRequiredService<IRepository<Staff>>();
    }

    [Test]
    public async Task AddAsync_ShouldCreateNewStaff()
    {
        var dto = new UpsertStaffDto
        {
            PlaceId = 1, // seeded
            Username = "serviceuser",
            Password = "secret",
            OIB = "55555555555",
            FirstName = "Service",
            LastName = "User",
            Role = EmployeeRole.regular
        };

        await _service.AddAsync(dto);

        var exists = await _staffRepo.ExistsAsync(s => s.Username == "serviceuser");
        Assert.That(exists, Is.True, "New staff should exist in the database after service call");
    }

    [Test]
    public async Task AddAsync_ShouldFail_WhenUsernameExists()
    {
        var existing = new Staff
        {
            PlaceId = 1,
            OIB = "77777777777",
            Username = "duplicateuser",
            Password = "pwd",
            FullName = "Dupe",
            Role = EmployeeRole.regular
        };
        await _staffRepo.AddAsync(existing);

        var dto = new UpsertStaffDto
        {
            PlaceId = 1,
            Username = "duplicateuser", // same
            Password = "new",
            OIB = "77777777777",
            FirstName = "New",
            LastName = "Try",
            Role = EmployeeRole.regular
        };

        var ex = Assert.ThrowsAsync<ConflictException>(() => _service.AddAsync(dto));
        Assert.That(ex?.Message, Does.Contain("already exists"));
    }
}
