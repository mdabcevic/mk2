using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests.Services;

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

    // ---- TESTS BELOW ---

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

    [Test]
    public void AddAsync_ShouldFail_WhenUserAccessesWrongPlace()
    {
        using var scope = Factory.Services.CreateScope();
        var mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
        mockUser.Override(new Staff { Id = 1, PlaceId = 99, Role = EmployeeRole.owner, OIB = "77777777777", Username = "none", FullName = "None None", Password = "hashed" });

        var service = scope.ServiceProvider.GetRequiredService<IStaffService>();

        var dto = new UpsertStaffDto
        {
            PlaceId = 1, // mismatches current user's place
            Username = "unauthorized",
            Password = "pwd",
            OIB = "00000000000",
            FirstName = "Denied",
            LastName = "User",
            Role = EmployeeRole.regular
        };

        var ex = Assert.ThrowsAsync<UnauthorizedBusinessAccessException>(() => service.AddAsync(dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldDeleteStaff_WhenAuthorized()
    {
        var staff = new Staff
        {
            PlaceId = 1,
            OIB = "32132132100",
            Username = "deleteauthorized",
            Password = "pwd",
            FullName = "Delete Me",
            Role = EmployeeRole.regular
        };
        await _staffRepo.AddAsync(staff);

        await _service.DeleteAsync(staff.Id);
        var result = await _staffRepo.GetByIdAsync(staff.Id);

        Assert.That(result, Is.Null, "Staff should be deleted from the database.");
    }

    [Test]
    public void DeleteAsync_ShouldFail_WhenStaffNotFound()
    {
        var ex = Assert.ThrowsAsync<StaffNotFoundException>(() => _service.DeleteAsync(9999));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public void DeleteAsync_ShouldFail_WhenUnauthorizedPlaceAccess()
    {
        var scope = Factory.Services.CreateScope();
        var mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
        mockUser.Override(new Staff
        {
            Id = 123,
            PlaceId = 99,
            Role = EmployeeRole.manager,
            OIB = "00000000000",
            Username = "wrongplace",
            FullName = "Wrong User",
            Password = "hashed"
        });

        var staff = new Staff
        {
            PlaceId = 1,
            OIB = "22233344455",
            Username = "targetuser",
            Password = "pwd",
            FullName = "Target User",
            Role = EmployeeRole.regular
        };

        _service = scope.ServiceProvider.GetRequiredService<IStaffService>();
        _staffRepo = scope.ServiceProvider.GetRequiredService<IRepository<Staff>>();

        _staffRepo.AddAsync(staff).Wait();

        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(() => _service.DeleteAsync(staff.Id));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnStaffDto_WhenAuthorized()
    {
        var staff = new Staff
        {
            PlaceId = 1,
            OIB = "55566677788",
            Username = "getme",
            Password = "pwd",
            FullName = "Get Me",
            Role = EmployeeRole.regular
        };
        await _staffRepo.AddAsync(staff);

        var result = await _service.GetByIdAsync(staff.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo("getme"));
    }

    [Test]
    public void GetByIdAsync_ShouldFail_WhenUnauthorizedPlaceAccess()
    {
        var scope = Factory.Services.CreateScope();
        var mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
        mockUser.Override(new Staff
        {
            Id = 321,
            PlaceId = 3,
            Role = EmployeeRole.owner,
            OIB = "99999999999",
            Username = "unauthorized",
            FullName = "Not Allowed",
            Password = "pwd"
        });

        _service = scope.ServiceProvider.GetRequiredService<IStaffService>();
        _staffRepo = scope.ServiceProvider.GetRequiredService<IRepository<Staff>>();

        var staff = new Staff
        {
            PlaceId = 1,
            OIB = "55500011122",
            Username = "protected",
            Password = "pwd",
            FullName = "Protected User",
            Role = EmployeeRole.regular
        };
        _staffRepo.AddAsync(staff).Wait();

        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(() => _service.GetByIdAsync(staff.Id));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void UpdateAsync_ShouldFail_WhenStaffNotFound()
    {
        var dto = new UpsertStaffDto
        {
            Id = 9999,
            PlaceId = 1,
            Username = "ghost",
            Password = "x",
            OIB = "00000000000",
            FirstName = "No",
            LastName = "Body",
            Role = EmployeeRole.regular
        };

        var ex = Assert.ThrowsAsync<StaffNotFoundException>(() => _service.UpdateAsync(9999, dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void UpdateAsync_ShouldFail_WhenUnauthorizedPlaceAccess()
    {
        var scope = Factory.Services.CreateScope();
        var mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
        mockUser.Override(new Staff
        {
            Id = 321,
            PlaceId = 99,
            Role = EmployeeRole.owner,
            OIB = "99999999999",
            Username = "unauthorized",
            FullName = "Not Allowed",
            Password = "pwd"
        });

        _service = scope.ServiceProvider.GetRequiredService<IStaffService>();
        _staffRepo = scope.ServiceProvider.GetRequiredService<IRepository<Staff>>();

        var staff = new Staff
        {
            PlaceId = 1,
            OIB = "12345678901",
            Username = "victim",
            Password = "pwd",
            FullName = "Victim",
            Role = EmployeeRole.regular
        };
        _staffRepo.AddAsync(staff).Wait();

        var dto = new UpsertStaffDto
        {
            Id = staff.Id,
            PlaceId = 1,
            Username = "victim",
            Password = "pwd",
            OIB = "12345678901",
            FirstName = "Updated",
            LastName = "Victim",
            Role = EmployeeRole.regular
        };

        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(() => _service.UpdateAsync(staff.Id, dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnOnlyStaffFromCurrentUsersPlace()
    {
        // Seed 2 staff for place 1
        await _staffRepo.AddAsync(new Staff
        {
            PlaceId = 1,
            OIB = "12345678901",
            Username = "sameplace1",
            Password = "x",
            FullName = "User1",
            Role = EmployeeRole.regular
        });

        await _staffRepo.AddAsync(new Staff
        {
            PlaceId = 1,
            OIB = "12345678902",
            Username = "sameplace2",
            Password = "x",
            FullName = "User2",
            Role = EmployeeRole.manager
        });

        // Seed 1 staff for different place
        await _staffRepo.AddAsync(new Staff
        {
            PlaceId = 5,
            OIB = "12345678903",
            Username = "differentplace",
            Password = "x",
            FullName = "ShouldNotAppear",
            Role = EmployeeRole.regular
        });

        var result = await _service.GetAllAsync();

        Assert.That(result, Has.Count.GreaterThan(2));
        Assert.That(result.Any(s => s.Username == "differentplace"), Is.False);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateStaff_WhenAuthorized()
    {
        // Arrange
        var staff = new Staff
        {
            PlaceId = 1,
            OIB = "11111111111",
            Username = "updateme",
            Password = "pwd",
            FullName = "Before Update",
            Role = EmployeeRole.regular
        };
        await _staffRepo.AddAsync(staff);

        var dto = new UpsertStaffDto
        {
            Id = staff.Id,
            PlaceId = 1,
            Username = "updateme", // must match for uniqueness
            Password = "updated-pwd",
            OIB = "11111111111",
            FirstName = "Updated",
            LastName = "Name",
            Role = EmployeeRole.manager
        };

        // Act
        await _service.UpdateAsync(staff.Id, dto);
        var updated = await _staffRepo.GetByIdAsync(staff.Id);

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.FullName, Is.EqualTo("Updated Name"));
        Assert.That(updated.Role, Is.EqualTo(EmployeeRole.manager));
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenUsernameAlreadyExists()
    {
        // Arrange
        var staff1 = new Staff
        {
            PlaceId = 1,
            OIB = "11111111111",
            Username = "originaluser",
            Password = "pwd",
            FullName = "Original",
            Role = EmployeeRole.regular
        };

        var staff2 = new Staff
        {
            PlaceId = 1,
            OIB = "22222222222",
            Username = "conflictuser",
            Password = "pwd",
            FullName = "Conflict",
            Role = EmployeeRole.manager
        };

        await _staffRepo.AddAsync(staff1);
        await _staffRepo.AddAsync(staff2);

        var dto = new UpsertStaffDto
        {
            Id = staff1.Id,
            PlaceId = 1,
            Username = "conflictuser", // duplicate username
            Password = "updated",
            OIB = "11111111111",
            FirstName = "Updated",
            LastName = "Name",
            Role = EmployeeRole.regular
        };

        // Act
        var ex = Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(staff1.Id, dto));

        // Assert
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex?.Message, Does.Contain("already exists"));
    }

}
