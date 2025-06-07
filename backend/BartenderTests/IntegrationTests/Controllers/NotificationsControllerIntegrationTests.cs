using System.Net.Http.Headers;
using System.Net;
using Bartender.Data;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using BartenderTests.Utility;


namespace BartenderTests.IntegrationTests.Controllers;

[TestFixture]
internal class NotificationsControllerIntegrationTests : IntegrationTestBase
{
    protected override bool UseMockCurrentUser => false;

    [Test]
    public async Task Get_ShouldReturnNotifications_WhenAuthorized()
    {
        var token = await LoginAndGetTokenAsync("vivasmanager", "test");
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await TestClient.GetAsync("/api/notifications?tableId=1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_ShouldClearNotifications_WhenAuthorized()
    {
        var token = await LoginAndGetTokenAsync("vivasmanager", "test");
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await TestClient.DeleteAsync("/api/notifications?tableId=1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    //[Test]
    //public async Task Patch_ShouldMarkAsRead_WhenAuthorized()
    //{
    //    var token = await LoginAndGetTokenAsync("vivasmanager", "test");
    //    TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    //    var table = TestDataFactory.CreateValidTable();

    //    // Simulate notification — optionally call real API that creates it
    //    var notification = new TableNotification
    //    {
    //        TableLabel = "T1",
    //        OrderId = null,
    //        Type = NotificationType.StaffNeeded,
    //        Message = "Guest is calling",
    //        Pending = true
    //    };

    //    // Add the notification directly via service (assuming DI works)
    //    using var scope = Factory.Services.CreateScope();
    //    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
    //    await notificationService.AddNotificationAsync(table, notification); // tableId = 1

    //    var notificationId = "notif:1:1:call"; // adjust to match seeded or real notification ID
    //    var response = await TestClient.PatchAsync($"/api/notifications/{notificationId}/mark-complete?tableId=1", null);

    //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    //}

    [Test]
    public async Task Get_ShouldReturnForbidden_WhenUnauthorizedRole()
    {
        var token = await LoginAndGetTokenAsync("testowner", "test");
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await TestClient.GetAsync("/api/notifications?tableId=1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}
