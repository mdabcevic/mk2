
using Bartender.Domain.DTO.Place;
using System.Net.Http.Json;
using System.Net;
using Bartender.Domain.DTO.Staff;
using System.Net.Http.Headers;

namespace BartenderTests.IntegrationTests.Controllers;

[TestFixture]
internal class PlacesControllerIntegrationTests : IntegrationTestBase
{
    protected override bool UseMockCurrentUser => false;

    private async Task AuthenticateAsAsync(string username, string password)
    {
        var loginDto = new LoginStaffDto { Username = username, Password = password };
        var loginResponse = await TestClient.PostAsJsonAsync("/api/auth", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var token = await loginResponse.Content.ReadAsStringAsync();
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Test]
    public async Task GetAll_ShouldReturnList_WhenCalledAnonymously()
    {
        var response = await TestClient.GetAsync("/api/places");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var places = await response.Content.ReadFromJsonAsync<List<PlaceDto>>();
        Assert.That(places, Is.Not.Null);
        Assert.That(places!.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetById_ShouldReturnPlace_WhenExists()
    {
        var response = await TestClient.GetAsync("/api/places/5");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    }

    //[Test]
    //public async Task Post_ShouldCreatePlace_WhenAdmin()
    //{
    //    await AuthenticateAsAsync("vivasadmin", "test123");

    //    var newPlace = new InsertPlaceDto
    //    {
    //        BusinessId = 1,
    //        CityId = 1,
    //        Address = "123 Test St",
    //        OpensAt = "9:00",
    //        ClosesAt = "12:00",
    //        Description = "Test place"
    //    };

    //    var response = await TestClient.PostAsJsonAsync("/api/places", newPlace);
    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    //}

    //[Test]
    //public async Task Put_ShouldUpdatePlace_WhenManager()
    //{
    //    await AuthenticateAsAsync("vivasmanager", "test");

    //    var updatedPlace = new UpdatePlaceDto
    //    {
    //        Address = "Updated Address",
    //        OpensAt = "9:00",
    //        ClosesAt = "12:00",
    //        Description = "Test place"
    //    };

    //    var response = await TestClient.PutAsJsonAsync("/api/places/1", updatedPlace);
    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    //}

    [Test]
    public async Task Delete_ShouldRemovePlace_WhenManager()
    {
        await AuthenticateAsAsync("vivasmanager", "test");

        var response = await TestClient.DeleteAsync("/api/places/1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Dashboard_ShouldReturnPlaceStatus_WhenManager()
    {
        await AuthenticateAsAsync("vivasmanager", "test");

        var response = await TestClient.GetAsync("/api/places/dashboard/1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var dashboard = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        Assert.Multiple(() =>
        {
            Assert.That(dashboard!.ContainsKey("activeOrders"));
            Assert.That(dashboard.ContainsKey("closedOrders"));
            Assert.That(dashboard.ContainsKey("freeTablesCount"));
        });
    }

    //[Test]
    //public async Task NotifyStaff_ShouldSucceed_WhenSaltValid()
    //{
    //    // Replace with an actual salt from the seeded place
    //    var response = await TestClient.GetAsync("/api/places/notify-staff/1b3593e63a6a4fef8f2e5eae19840165");

    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    //}

    //[Test]
    //public async Task Post_ShouldFail_WhenNotAdmin()
    //{
    //    await AuthenticateAsAsync("vivasmanager", "test"); // not admin

    //    var newPlace = new InsertPlaceDto
    //    {
    //        BusinessId = 1,
    //        CityId = 1,
    //        Address = "Test",
    //        OpensAt = "09:00",
    //        ClosesAt = "18:00",
    //        Description = "Invalid role test"
    //    };

    //    var response = await TestClient.PostAsJsonAsync("/api/places", newPlace);
    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    //}

    //[Test]
    //public async Task Post_ShouldFail_WhenMissingFields()
    //{
    //    await AuthenticateAsAsync("vivasadmin", "test123");

    //    var incomplete = new InsertPlaceDto
    //    {
    //        BusinessId = 1, // Missing required fields
    //        CityId = 1,
    //        Address = "Something"
    //    };

    //    var response = await TestClient.PostAsJsonAsync("/api/places", incomplete);
    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    //}

    [Test]
    public async Task GetById_ShouldReturnNotFound_WhenIdInvalid()
    {
        var response = await TestClient.GetAsync("/api/places/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task NotifyStaff_ShouldReturnNotFound_WhenSaltInvalid()
    {
        var response = await TestClient.GetAsync("/api/places/notify-staff/invalidsalt123");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

}
