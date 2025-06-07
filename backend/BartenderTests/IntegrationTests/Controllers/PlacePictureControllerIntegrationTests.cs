using System.Net.Http.Json;
using System.Net;
using Bartender.Data.Enums;
using Bartender.Domain.DTO.PlaceImage;
using Bartender.Domain.DTO.Staff;
using System.Net.Http.Headers;

namespace BartenderTests.IntegrationTests.Controllers;

[TestFixture]
internal class PlacePictureControllerIntegrationTests : IntegrationTestBase
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
    public async Task GetImagesByPlace_ShouldReturnVisibleImages_WhenCalledAnonymously()
    {
        var response = await TestClient.GetAsync("/api/images/1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    }

    [Test]
    public async Task Post_ShouldCreateImage_WhenAuthorized()
    {
        await AuthenticateAsAsync("vivasmanager", "test");

        var newImage = new UpsertImageDto
        {
            PlaceId = 1,
            Url = "https://example.com/test-image.jpg",
            ImageType = ImageType.gallery,
        };

        var response = await TestClient.PostAsJsonAsync("/api/images", newImage);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Put_ShouldUpdateImage_WhenAuthorized()
    {
        await AuthenticateAsAsync("vivasmanager", "test");

        var updated = new UpsertImageDto
        {
            PlaceId = 1,
            Url = "https://example.com/updated.jpg",
            ImageType = ImageType.banner,
        };
        await TestClient.PostAsJsonAsync("/api/images", updated);

        updated.ImageType = ImageType.blueprints;

        var response = await TestClient.PutAsJsonAsync("/api/images/1", updated);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Delete_ShouldRemoveImage_WhenAuthorized()
    {
        await AuthenticateAsAsync("vivasmanager", "test");

        var response = await TestClient.DeleteAsync("/api/images/1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}
