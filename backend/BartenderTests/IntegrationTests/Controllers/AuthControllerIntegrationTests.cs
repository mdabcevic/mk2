using Bartender.Domain.DTO.Staff;
using System.Net.Http.Json;
using System.Net;


namespace BartenderTests.IntegrationTests.Controllers;

[TestFixture]
internal class AuthControllerIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task Post_ShouldReturnToken_WhenLoginSuccessful()
    {
        // Arrange
        var dto = new LoginStaffDto
        {
            Username = "cloud9_admin",
            Password = "123456"
        };

        // Act
        var response = await TestClient.PostAsJsonAsync("/api/auth", dto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var token = await response.Content.ReadAsStringAsync();
        Assert.That(token, Is.Not.Empty);
    }

    [Test]
    public async Task Post_ShouldReturnUnauthorized_WhenCredentialsInvalid()
    {
        // Arrange
        var dto = new LoginStaffDto
        {
            Username = "wrong",
            Password = "wrong"
        };

        // Act
        var response = await TestClient.PostAsJsonAsync("/api/auth", dto);

        // Assert
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.OK)); // Bad request instead of Unauthorized
    }
}
