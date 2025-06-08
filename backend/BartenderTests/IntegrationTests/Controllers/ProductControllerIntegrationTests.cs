using Bartender.Domain.DTO.Product;
using Bartender.Domain.DTO.Staff;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BartenderTests.IntegrationTests.Controllers;

[TestFixture]
internal class ProductControllerIntegrationTests : IntegrationTestBase
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
    public async Task GetById_ShouldReturnProduct_WhenExists()
    {
        await AuthenticateAsAsync("vivasmanager", "test");
        var response = await TestClient.GetAsync("/api/product/1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAll_ShouldReturnProducts_WhenAuthorized()
    {
        await AuthenticateAsAsync("vivasmanager", "test");
        var response = await TestClient.GetAsync("/api/product");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetProductCategories_ShouldReturnCategories_Anonymously()
    {
        var response = await TestClient.GetAsync("/api/product/categories");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Search_ShouldReturnFilteredProducts_WhenParamsMatch()
    {
        await AuthenticateAsAsync("vivasmanager", "test");
        var response = await TestClient.GetAsync("/api/product/search?name=espresso");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Post_ShouldAddProduct_WhenValid()
    {
        await AuthenticateAsAsync("vivasmanager", "test");

        var dto = new UpsertProductDto
        {
            Name = "New Coffee",
            Volume = "1L",
            CategoryId = 1,
            BusinessId = 1
        };

        var response = await TestClient.PostAsJsonAsync("/api/product", dto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    //[Test]
    //public async Task Put_ShouldUpdateProduct_WhenValid()
    //{
    //    await AuthenticateAsAsync("vivadmin", "test123");

    //    var dto = new UpsertProductDto
    //    {
    //        Name = "Espres",
    //        Volume = "2L",
    //    };

    //    var response = await TestClient.PutAsJsonAsync("/api/product/1", dto);
    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    //}

    [Test]
    public async Task Delete_ShouldRemoveProduct_WhenExists()
    {
        await AuthenticateAsAsync("vivasadmin", "test123");

        var response = await TestClient.DeleteAsync("/api/product/1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}
