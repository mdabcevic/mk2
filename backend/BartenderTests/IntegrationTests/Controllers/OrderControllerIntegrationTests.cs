using Bartender.Domain.DTO.Staff;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BartenderTests.IntegrationTests.Controllers;

[TestFixture]
internal class OrderControllerIntegrationTests : IntegrationTestBase
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
    public async Task GetById_ShouldReturnOrder_WhenExists()
    {
        var response = await TestClient.GetAsync("/api/order/1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetTableOrders_ShouldReturnOrders_WhenAuthorized()
    {
        await AuthenticateAsAsync("vivasmanager", "test");

        var response = await TestClient.GetAsync("/api/order/table-orders/T1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAllActiveOrders_ShouldReturnList_WhenAuthorized()
    {
        await AuthenticateAsAsync("vivasmanager", "test");

        var response = await TestClient.GetAsync("/api/order/active/1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    //[Test]
    //public async Task UpdateOrder_ShouldSucceed_WhenAuthorized()
    //{
    //    await AuthenticateAsAsync("vivasmanager", "test");

    //    var dto = new UpsertOrderDto
    //    {
    //        TableId = 1,
    //        TotalPrice = 10,
    //        PaymentType = PaymentType.cash,
    //        Items =
    //        [
    //            new() { MenuItemId = 1, Count = 1 } // Include Price if required
    //        ]
    //    };

    //    var response = await TestClient.PutAsJsonAsync("/api/order/1", dto);

    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    //}

    //[Test]
    //public async Task DeleteOrder_ShouldSucceed_WhenAuthorizedAndCancelled()
    //{
    //    await AuthenticateAsAsync("vivasadmin", "test123");

    //    var response = await TestClient.DeleteAsync("/api/order/1");

    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    //}

    [Test]
    public async Task GetAllClosedOrdersByPlace_ShouldReturnResults_WhenAuthorized()
    {
        await AuthenticateAsAsync("vivasadmin", "test123");

        var response = await TestClient.GetAsync("/api/order/closed/1?page=1&size=10");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAllActiveOrdersByPlace_ShouldReturnGroupedResults_WhenGrouped()
    {
        await AuthenticateAsAsync("vivasmanager", "test");

        var response = await TestClient.GetAsync("/api/order/active/1?grouped=true&page=1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAllByBusiness_ShouldReturnOrders_WhenAuthorized()
    {
        await AuthenticateAsAsync("vivasadmin", "test123");

        var response = await TestClient.GetAsync("/api/order/business/1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

}
