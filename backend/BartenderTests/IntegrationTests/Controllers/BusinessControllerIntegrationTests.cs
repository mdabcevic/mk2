using Bartender.Data.Enums;
using Bartender.Domain.DTO.Business;
using BartenderTests.Utility;
using System.Net.Http.Json;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Bartender.Domain.DTO.Staff;
using System.Net.Http.Headers;
using Bartender.Data.Models;


namespace BartenderTests.IntegrationTests.Controllers;

[TestFixture]
internal class BusinessControllerIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task GetById_ShouldReturnBusiness_WhenAuthorized()
    {
        using var scope = Factory.Services.CreateScope();
        var mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();

        var staff = TestDataFactory.CreateValidStaff(
        role: EmployeeRole.owner,
        businessid: 1,
        placeid: 1
        );

        mockUser.Override(staff);

        var response = await TestClient.GetAsync($"/api/business/{1}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAll_ShouldReturnList_WhenUserIsOwner()
    {
        var loginDto = new LoginStaffDto
        {
            Username = "testowner",
            Password = "test"
        };

        var loginResponse = await TestClient.PostAsJsonAsync("/api/auth", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        var tokenString = await loginResponse.Content.ReadAsStringAsync(); // assuming your login returns plain string
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);

        var response = await TestClient.GetAsync("/api/business");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_ShouldCreateBusiness()
    {
        var dto = new UpsertBusinessDto
        {
            Name = "NewBiz",
            Headquarters = "Zagreb",
            OIB = "01234567890"
        };

        var response = await TestClient.PostAsJsonAsync("/api/business", dto);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    //[Test]
    //public async Task Put_ShouldUpdateBusiness_WhenAuthorized()
    //{
    //    using var scope = Factory.Services.CreateScope();
    //    var mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
    //    var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.owner, businessid: 1, placeid: 1);
    //    mockUser.Override(staff);

    //    var dto = new UpsertBusinessDto
    //    {
    //        Name = "Updated",
    //        Headquarters = "Split",
    //        OIB = "01234567890"
    //    };

    //    var response = await TestClient.PutAsJsonAsync($"/api/business/{1}", dto);

    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    //}

    //[Test]
    //public async Task Patch_ShouldUpdateSubscription_WhenAdmin()
    //{
    //    var loginDto = new LoginStaffDto
    //    {
    //        Username = "vivasadmin",
    //        Password = "test123"
    //    };

    //    var loginResponse = await TestClient.PostAsJsonAsync("/api/auth", loginDto);
    //    loginResponse.EnsureSuccessStatusCode();

    //    var tokenString = await loginResponse.Content.ReadAsStringAsync(); // assuming your login returns plain string
    //    TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);

    //    var response = await TestClient.PatchAsJsonAsync("/api/business/subscription", SubscriptionTier.premium);

    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    //}

    [Test]
    public async Task GetAll_ShouldFail_WhenUnauthorized()
    {
        using var scope = Factory.Services.CreateScope();
        var mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
        var staff = TestDataFactory.CreateValidStaff(
        role: EmployeeRole.regular,
        businessid: 1,
        placeid: 1
        );

        mockUser.Override(staff);

        var response = await TestClient.GetAsync("/api/business");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Put_ShouldFail_WhenUnauthorizedRole()
    {
        SetAuthHeader(role: "regular");
        using var scope = Factory.Services.CreateScope();
        var mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
        var staff = TestDataFactory.CreateValidStaff(
        role: EmployeeRole.regular,
        businessid: 1,
        placeid: 1
        );

        mockUser.Override(staff);

        var dto = new UpsertBusinessDto
        {
            Name = "Invalid",
            Headquarters = "Nowhere",
            OIB = "00000000000"
        };

        var response = await TestClient.PutAsJsonAsync("/api/business/1", dto);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private async Task<string> LoginAndGetTokenAsync(string username, string password)
    {
        var response = await TestClient.PostAsJsonAsync("/api/auth", new LoginStaffDto
        {
            Username = username,
            Password = password
        });

        response.EnsureSuccessStatusCode(); // Optional: throw if bad

        var result = await response.Content.ReadAsStringAsync();
        return result;
    }

    private record TokenResponse(string AccessToken, DateTime ExpiresAt);

}
