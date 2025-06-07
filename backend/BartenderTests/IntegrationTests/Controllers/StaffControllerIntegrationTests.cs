using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;

namespace BartenderTests.IntegrationTests.Controllers;

[TestFixture]
public class StaffControllerIntegrationTests : IntegrationTestBase
{
    private IRepository<Staff> _staffRepo = null!;

    [SetUp]
    public void Setup()
    {
        var scope = Factory.Services.CreateScope();
        _staffRepo = scope.ServiceProvider.GetRequiredService<IRepository<Staff>>();
    }

    private static UpsertStaffDto BuildValidStaffDto(string username = "controlleruser") => new()
    {
        PlaceId = 1,
        Username = username,
        Password = "pwd",
        OIB = "00000000000",
        FirstName = "From",
        LastName = "Controller",
        Role = EmployeeRole.regular
    };

    [Test]
    public async Task Post_ShouldCreateNewStaff()
    {
        // Arrange
        var dto = BuildValidStaffDto();

        // Act
        var response = await TestClient.PostAsJsonAsync("/api/staff", dto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var exists = await _staffRepo.ExistsAsync(s => s.Username == dto.Username);
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task Get_ShouldReturnAllStaff()
    {
        // Act
        var response = await TestClient.GetAsync("/api/staff");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    //[Test]
    //public async Task Put_ShouldUpdateStaff()
    //{
    //    var staff = new Staff
    //    {
    //        PlaceId = 1,
    //        Username = "update_me",
    //        Password = "pwd",
    //        OIB = "11111111111",
    //        FullName = "Before",
    //        Role = EmployeeRole.regular
    //    };
    //    await _staffRepo.AddAsync(staff);

    //    var dto = new UpsertStaffDto
    //    {
    //        Id = staff.Id,
    //        PlaceId = 1,
    //        Username = "update_me",
    //        Password = "newpwd",
    //        OIB = "11111111111",
    //        FirstName = "Updated",
    //        LastName = "User",
    //        Role = EmployeeRole.manager
    //    };

    //    var response = await TestClient.PutAsJsonAsync($"/api/staff/{staff.Id}", dto);

    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

    //    var updated = await _staffRepo.GetByIdAsync(staff.Id);
    //    Assert.That(updated!.FullName, Is.EqualTo("Updated User"));
    //}

    [Test]
    public async Task Delete_ShouldRemoveStaff()
    {
        // Arrange
        var staff = new Staff
        {
            PlaceId = 1,
            Username = "delete_me",
            Password = "pwd",
            OIB = "33333333333",
            FullName = "Delete Me",
            Role = EmployeeRole.regular
        };
        await _staffRepo.AddAsync(staff);

        // Act
        var response = await TestClient.DeleteAsync($"/api/staff/{staff.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var deleted = await _staffRepo.GetByIdAsync(staff.Id);
        Assert.That(deleted, Is.Null);
    }

}
