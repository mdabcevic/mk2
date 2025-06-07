using Bartender.Domain.DTO.MenuItem;
using System.Net.Http.Json;
using System.Net;
using Bartender.Domain.DTO.Staff;
using System.Net.Http.Headers;


namespace BartenderTests.IntegrationTests.Controllers;

[TestFixture]
internal class MenuItemControllerIntegrationTests : IntegrationTestBase
{
    protected override bool UseMockCurrentUser => false;

    [Test]
    public async Task GetById_ShouldReturnItem_WhenExists()
    {
        var response = await TestClient.GetAsync("/api/menu/1/1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var dto = await response.Content.ReadFromJsonAsync<MenuItemDto>();
        Assert.That(dto, Is.Not.Null);
        //Assert.That(dto.ProductId, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByPlaceId_ShouldReturnList_WhenExists()
    {
        var response = await TestClient.GetAsync("/api/menu/1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetByPlaceIdGrouped_ShouldReturnGrouped_WhenExists()
    {
        var response = await TestClient.GetAsync("/api/menu/1?groupByCategory=true");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        //var grouped = await response.Content.ReadFromJsonAsync<List<MenuItemsByCategoryDto>>();
        //Assert.That(grouped, Is.Not.Null);
    }

    [Test]
    public async Task Search_ShouldReturnMatches()
    {
        var response = await TestClient.GetAsync("/api/menu/search?placeId=1&searchProduct=cappuccino");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_ShouldAddMenuItems_WhenAuthorized()
    {
        var loginDto = new LoginStaffDto { Username = "vivasadmin", Password = "test123" };
        var loginResponse = await TestClient.PostAsJsonAsync("/api/auth", loginDto);
        var token = await loginResponse.Content.ReadAsStringAsync();
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var newItems = new List<UpsertMenuItemDto>
        {
            new()
            {
                PlaceId = 3,
                ProductId = 50, // make sure productId=2 exists
                Price = 3.5m,
                IsAvailable = true
            }
        };

        var response = await TestClient.PostAsJsonAsync("/api/menu", newItems);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Put_ShouldUpdateMenuItem_WhenAuthorized()
    {
        var loginDto = new LoginStaffDto { Username = "vivasmanager", Password = "test" };
        var loginResponse = await TestClient.PostAsJsonAsync("/api/auth", loginDto);
        var token = await loginResponse.Content.ReadAsStringAsync();
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateDto = new UpsertMenuItemDto
        {
            PlaceId = 1,
            ProductId = 1,
            Price = 12.0m,
            IsAvailable = false
        };

        var response = await TestClient.PutAsJsonAsync("/api/menu", updateDto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Delete_ShouldRemoveMenuItem_WhenAuthorized()
    {
        var loginDto = new LoginStaffDto { Username = "vivasmanager", Password = "test" };
        var loginResponse = await TestClient.PostAsJsonAsync("/api/auth", loginDto);
        var token = await loginResponse.Content.ReadAsStringAsync();
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await TestClient.DeleteAsync("/api/menu/1/1"); // placeId=1, productId=1 must exist
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CopyMenu_ShouldCopyItems_WhenAuthorized()
    {
        var token = await LoginAndGetTokenAsync("vivasadmin", "test123");
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await TestClient.PostAsync("/api/menu/1/2/copy", null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var check = await TestClient.GetAsync("/api/menu/2");
        Assert.That(check.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    protected async Task<string> LoginAndGetTokenAsync(string username, string password)
    {
        var loginDto = new LoginStaffDto { Username = username, Password = password };
        var response = await TestClient.PostAsJsonAsync("/api/auth", loginDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

}
