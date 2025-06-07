using Bartender.Data.Enums;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.DTO.Table;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;

namespace BartenderTests.IntegrationTests.Controllers;

[TestFixture]
internal class TableControllerIntegrationTests : IntegrationTestBase
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
    public async Task GetAll_ShouldReturnList()
    {
        await AuthenticateAsAsync("vivasmanager", "test");
        var response = await TestClient.GetAsync("/api/tables");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    //[Test]
    //public async Task GetById_ShouldReturnTable_WhenExists()
    //{
    //    await AuthenticateAsAsync("vivasmanager", "test");
    //    var response = await TestClient.GetAsync("/api/tables/1");
    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    //}

    [Test]
    public async Task GetByPlaceId_ShouldReturnTables_WhenAnonymous()
    {
        var response = await TestClient.GetAsync("/api/tables/1/all");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    //[Test]
    //public async Task GetBySalt_ShouldSucceed_ForGuest()
    //{
    //    var salt = "5036144c6f5d41aeb0e332ea0029e073";
    //    var response = await TestClient.GetAsync($"/api/tables/lookup?salt={salt}");
    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    //}

    //[Test]
    //public async Task ChangeStatus_ShouldSucceed_ForGuest()
    //{
    //    var guestToken = "replace-me"; // Replace with actual guest token from seeded session
    //    var response = await TestClient.PatchAsJsonAsync($"/api/tables/{guestToken}/status", TableStatus.empty);
    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    //}

    [Test]
    public async Task BulkUpsert_ShouldSucceed_WhenManager()
    {
        await AuthenticateAsAsync("vivasmanager", "test");

        var tables = new List<UpsertTableDto>
        {
            new() { Label = "T200", Width = 4, Height = 4, X = 4, Y = 4, Seats = 4 },
            new() { Label = "T201", Width = 4, Height = 4, X = 4, Y = 4, Seats = 4 }
        };

        var response = await TestClient.PostAsJsonAsync("/api/tables/bulk-upsert", tables);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Delete_ShouldRemove_WhenManager()
    {
        await AuthenticateAsAsync("vivasmanager", "test");
        var response = await TestClient.DeleteAsync("/api/tables/1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    //[Test]
    //public async Task RotateToken_ShouldReturnNewSalt_WhenManager()
    //{
    //    await AuthenticateAsAsync("vivasmanager", "test");
    //    var response = await TestClient.PostAsync("/api/tables/1/rotate-token", null);
    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    //}

    //[Test]
    //public async Task ToggleDisabled_ShouldUpdateState_WhenManager()
    //{
    //    await AuthenticateAsAsync("vivasmanager", "test");
    //    var response = await TestClient.PatchAsJsonAsync("/api/tables/1/toggle-disabled", true);
    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    //}
}
