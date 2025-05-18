using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Table;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Bartender.Domain.Utility.Exceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BartenderTests.IntegrationTests;

[TestFixture]
public class TableInteractionServiceIntegrationTests : IntegrationTestBase
{
    private ITableInteractionService _service = null!;
    private IRepository<Table> _tableRepo = null!;
    private MockCurrentUser _mockUser = null!;
    private IGuestSessionService _guestSessionService = null!;
    private ITableSessionService _tableSessionService = null!;
    private INotificationService _notificationService = null!;
    private IOrderRepository _orderRepo = null!;
    private IRepository<GuestSession> _guestSessionRepo = null!;

    [SetUp]
    public void SetUp()
    {
        var scope = Factory.Services.CreateScope();
        _service = scope.ServiceProvider.GetRequiredService<ITableInteractionService>();
        _tableRepo = scope.ServiceProvider.GetRequiredService<IRepository<Table>>();
        _mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
        _guestSessionService = scope.ServiceProvider.GetRequiredService<IGuestSessionService>();
        _tableSessionService = scope.ServiceProvider.GetRequiredService<ITableSessionService>();
        _notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        _orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        _guestSessionRepo = scope.ServiceProvider.GetRequiredService<IRepository<GuestSession>>();
    }

    [Test]
    public async Task GetBySaltAsync_ShouldReturnTable_WhenScannedByStaff()
    {
        // Arrange
        var table = new Table { PlaceId = 1, Label = "TBL1", Status = TableStatus.empty, QrSalt = "some-salt",
            Width = 100,
            Height = 100,
            X = 100,
            Y = 100
        };
        await _tableRepo.AddAsync(table);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1, role: EmployeeRole.regular));
        TableScanDto? result = null;

        // Act & Assert
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await _service.GetBySaltAsync(table.QrSalt);
        });

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Label, Is.EqualTo("TBL1"));
            Assert.That(result.IsSessionEstablished, Is.False); // Only guests get a session
            Assert.That(result.GuestToken, Is.Null);            // Staff doesn't get a guest token
        });

        var updated = await _tableRepo.GetByIdAsync(table.Id);
        Assert.That(updated!.Status, Is.EqualTo(TableStatus.occupied));
    }

    [Test]
    public void GetBySaltAsync_ShouldFail_WhenTableNotFound()
    {
        // Arrange
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.GetBySaltAsync("invalid-token"));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("Invalid QR code"));
    }

    [Test]
    public async Task GetBySaltAsync_ShouldBlockDisabledTable_ForGuest()
    {
        // Arrange
        var table = new Table
        {
            PlaceId = 1,
            Label = "D1",
            Status = TableStatus.empty,
            IsDisabled = true,
            Width = 100,
            Height = 100,
            X = 50,
            Y = 50
        };
        await _tableRepo.AddAsync(table);

        _mockUser.OverrideGuest("guest-token");

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetBySaltAsync(table.QrSalt));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("unavailable"));
    }

    [Test]
    public async Task GetBySaltAsync_ShouldCreateSession_WhenGuestScansEmptyTable()
    {
        // Arrange
        var table = new Table
        {
            Label = "Q1",
            PlaceId = 1,
            Status = TableStatus.empty,
            QrSalt = "qr-guest-1",
            Width = 100,
            Height = 100,
            X = 100,
            Y = 100
        };
        await _tableRepo.AddAsync(table);

        _mockUser.OverrideGuest("new-guest-token");

        // Act
        var result = await _service.GetBySaltAsync("qr-guest-1");

        // Assert basic response
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSessionEstablished, Is.True);
            Assert.That(result.GuestToken, Is.Not.Null.Or.Empty);
            Assert.That(result.Label, Is.EqualTo("Q1"));
        });

        // Assert DB state
        var sessions = await _guestSessionRepo.GetFilteredAsync(filterBy: g => g.TableId == table.Id && g.Token == result.GuestToken);
        Assert.That(sessions, Has.Count.EqualTo(1));

        // Assert table is now marked occupied
        var updated = await _tableRepo.GetByIdAsync(table.Id);
        Assert.That(updated!.Status, Is.EqualTo(TableStatus.occupied));
    }

    [Test]
    public async Task GetBySaltAsync_ShouldResumeSession_WhenGuestHasValidSession()
    {
        var table = new Table { PlaceId = 1, Label = "G1", Status = TableStatus.occupied, QrSalt = "guest-resume",
            Width = 100,
            Height = 100,
            X = 100,
            Y = 100
        };
        await _tableRepo.AddAsync(table);

        _mockUser.OverrideGuest("valid-token");

        await _guestSessionRepo.AddAsync(new GuestSession { TableId = table.Id, Token = "valid-token" });

        var result = await _service.GetBySaltAsync("guest-resume");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSessionEstablished, Is.True);
            Assert.That(result.GuestToken, Is.EqualTo("valid-token"));
            Assert.That(result.Label, Is.EqualTo("G1"));
        });
    }

    [Test]
    public async Task GetBySaltAsync_ShouldRequestPassphrase_WhenTableIsOccupied()
    {
        // Arrange
        var table = new Table
        {
            PlaceId = 1,
            Label = "OCC1",
            Status = TableStatus.occupied,
            QrSalt = "occupied",
            Width = 100,
            Height = 100,
            X = 100,
            Y = 100
        };
        await _tableRepo.AddAsync(table);

        // Override guest with no active session
        _mockUser.OverrideGuest("guest-token");

        // Ensure no existing guest session exists for this guest
        var activeSessions = await _guestSessionRepo.GetFilteredAsync(
            filterBy: g => g.Token == "guest-token");
        foreach (var session in activeSessions)
            await _guestSessionRepo.DeleteAsync(session);

        // Act
        var result = await _service.GetBySaltAsync("occupied");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSessionEstablished, Is.False);
            Assert.That(result.GuestToken, Is.Null.Or.Empty);
            Assert.That(result.Message, Does.Contain("passphrase"));
        });
    }

    [Test]
    public async Task GetBySaltAsync_ShouldJoinExistingSession_WhenCorrectPassphrase()
    {
        // Arrange
        var table = new Table
        {
            PlaceId = 1,
            Label = "JOIN1",
            Status = TableStatus.occupied,
            QrSalt = "join-pass",
            Width = 100,
            Height = 100,
            X = 100,
            Y = 100
        };
        await _tableRepo.AddAsync(table);

        // Setup guest A to start the session with a shared passphrase
        var starterGuestToken = "starter-token";
        _mockUser.OverrideGuest(starterGuestToken);

        var firstScan = await _service.GetBySaltAsync("join-pass");
        var groupPass = await _guestSessionRepo
            .GetByKeyAsync(g => g.Token == firstScan.GuestToken);

        Assert.That(groupPass, Is.Not.Null);

        // Switch to a second guest
        var joiningGuestToken = "joining-guest";
        _mockUser.OverrideGuest(joiningGuestToken);

        // Act
        var result = await _service.GetBySaltAsync("join-pass", groupPass!.Group!.Passphrase);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSessionEstablished, Is.True);
            Assert.That(result.GuestToken, Is.Not.EqualTo(starterGuestToken));
            Assert.That(result.GuestToken, Is.Not.Null.And.Not.Empty);
        });

        var sessionCount = await _guestSessionRepo.CountAsync(g => g.TableId == table.Id);
        Assert.That(sessionCount, Is.EqualTo(2));
    }


    [Test]
    public async Task GetBySaltAsync_ShouldFail_WhenIncorrectPassphrase()
    {
        // Arrange
        var table = new Table
        {
            PlaceId = 1,
            Label = "FAIL1",
            Status = TableStatus.occupied,
            QrSalt = "fail-qr",
            Width = 100,
            Height = 100,
            X = 100,
            Y = 100
        };
        await _tableRepo.AddAsync(table);

        // Start valid session with Guest A
        var starterToken = "starter-token";
        _mockUser.OverrideGuest(starterToken);

        var firstScan = await _service.GetBySaltAsync("fail-qr");
        var actualPassphrase = await _guestSessionRepo
            .GetByKeyAsync(g => g.Token == firstScan.GuestToken);

        Assert.That(actualPassphrase, Is.Not.Null);

        // Switch to Guest B using wrong passphrase
        var wrongToken = "wrong-token";
        _mockUser.OverrideGuest(wrongToken);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GetBySaltAsync("fail-qr", "WRONG-PASSPHRASE"));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("passphrase"));

        // Ensure only one session still exists
        var sessionCount = await _guestSessionRepo.CountAsync(g => g.TableId == table.Id);
        Assert.That(sessionCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ChangeStatusAsync_ShouldFreeTable_WhenGuestHasValidToken()
    {
        var table = new Table
        {
            PlaceId = 1,
            Label = "FREE1",
            Status = TableStatus.occupied,
            QrSalt = "free-qr",
            Width = 100,
            Height = 100,
            X = 50,
            Y = 50
        };
        await _tableRepo.AddAsync(table);

        var guest = new GuestSession { TableId = table.Id, Token = "guest-free" };
        await _guestSessionRepo.AddAsync(guest);

        _mockUser.OverrideGuest("guest-free");

        await _service.ChangeStatusAsync("free-qr", TableStatus.empty);

        var updated = await _tableRepo.GetByIdAsync(table.Id);
        Assert.That(updated!.Status, Is.EqualTo(TableStatus.empty));

        var activeSessions = await _guestSessionRepo.GetFilteredAsync(
            filterBy: g => g.TableId == table.Id && g.IsValid);
        Assert.That(activeSessions, Is.Empty);
    }

    [Test]
    public async Task ChangeStatusAsync_ShouldNoOp_WhenTableAlreadyEmpty()
    {
        var table = new Table
        {
            PlaceId = 1,
            Label = "NOP1",
            Status = TableStatus.empty,
            QrSalt = "noop-qr",
            Width = 100,
            Height = 100,
            X = 50,
            Y = 50
        };
        await _tableRepo.AddAsync(table);

        var guest = new GuestSession { TableId = table.Id, Token = "noop-guest" };
        await _guestSessionRepo.AddAsync(guest);

        _mockUser.OverrideGuest("noop-guest");

        Assert.DoesNotThrowAsync(() => _service.ChangeStatusAsync("noop-qr", TableStatus.empty));
    }

    [Test]
    public async Task ChangeStatusAsync_ShouldFail_WhenNoGuestTokenProvided()
    {
        var table = new Table { PlaceId = 1, QrSalt = "missing-token", Status = TableStatus.occupied,
            Width = 100,
            Height = 100,
            X = 50,
            Y = 50
        };
        await _tableRepo.AddAsync(table);

        _mockUser.OverrideGuest(null); // No token

        var ex = Assert.ThrowsAsync<AuthorizationException>(() =>
            _service.ChangeStatusAsync("missing-token", TableStatus.empty));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("Missing authentication token"));
    }

    [Test]
    public async Task ChangeStatusAsync_ShouldFail_WhenGuestTriesToSetNonEmptyStatus()
    {
        var table = new Table { PlaceId = 1, QrSalt = "invalid-change", Status = TableStatus.occupied,
            Width = 100,
            Height = 100,
            X = 50,
            Y = 50
        };
        await _tableRepo.AddAsync(table);

        var guest = new GuestSession { TableId = table.Id, Token = "bad-guest" };
        await _guestSessionRepo.AddAsync(guest);

        _mockUser.OverrideGuest("bad-guest");

        var ex = Assert.ThrowsAsync<AuthorizationException>(() =>
            _service.ChangeStatusAsync("invalid-change", TableStatus.reserved));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("Guests can only free tables"));
    }

    [Test]
    public async Task ChangeStatusAsync_ShouldFreeTable_WhenGuestHasValidSession()
    {
        var table = new Table
        {
            Label = "FREE1",
            PlaceId = 1,
            Status = TableStatus.occupied,
            QrSalt = "free-qr",
            Width = 100,
            Height = 100,
            X = 100,
            Y = 100
        };
        await _tableRepo.AddAsync(table);

        var guest = new GuestSession { TableId = table.Id, Token = "free-token" };
        await _guestSessionRepo.AddAsync(guest);

        _mockUser.OverrideGuest("free-token");

        await _service.ChangeStatusAsync("free-qr", TableStatus.empty);

        var updated = await _tableRepo.GetByIdAsync(table.Id);
        Assert.That(updated!.Status, Is.EqualTo(TableStatus.empty));

        var activeSessions = await _guestSessionRepo.GetFilteredAsync(
            filterBy: g => g.TableId == table.Id && g.IsValid);
        Assert.That(activeSessions, Is.Empty);
    }

    [Test]
    public async Task ChangeStatusAsync_ShouldNotFail_WhenTableAlreadyEmpty()
    {
        var table = new Table
        {
            Label = "EMPTY1",
            PlaceId = 1,
            Status = TableStatus.empty,
            QrSalt = "already-empty",
            Width = 100,
            Height = 100,
            X = 50,
            Y = 50
        };
        await _tableRepo.AddAsync(table);

        var guest = new GuestSession { TableId = table.Id, Token = "noop-token" };
        await _guestSessionRepo.AddAsync(guest);

        _mockUser.OverrideGuest("noop-token");

        Assert.DoesNotThrowAsync(() =>
            _service.ChangeStatusAsync("already-empty", TableStatus.empty));
    }

    [Test]
    public async Task ChangeStatusAsync_ShouldFail_WhenGuestHasNoToken()
    {
        var table = new Table { Label = "NT1", PlaceId = 1, Status = TableStatus.occupied, QrSalt = "no-token",
            Width = 100,
            Height = 100,
            X = 50,
            Y = 50
        };
        await _tableRepo.AddAsync(table);

        _mockUser.OverrideGuest(null); // simulate missing token

        var ex = Assert.ThrowsAsync<AuthorizationException>(() =>
            _service.ChangeStatusAsync("no-token", TableStatus.empty));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("authentication token"));
    }

    [Test]
    public async Task ChangeStatusAsync_ShouldFail_WhenGuestSetsNonEmptyStatus()
    {
        var table = new Table {Label = "INV1", PlaceId = 1, Status = TableStatus.occupied, QrSalt = "invalid-guest", Width = 100, Height = 100, X = 50, Y = 50 };
        await _tableRepo.AddAsync(table);

        var guest = new GuestSession { TableId = table.Id, Token = "invalid-token" };
        await _guestSessionRepo.AddAsync(guest);

        _mockUser.OverrideGuest("invalid-token");

        var ex = Assert.ThrowsAsync<AuthorizationException>(() =>
            _service.ChangeStatusAsync("invalid-guest", TableStatus.occupied));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("only free tables"));
    }

    [Test]
    public async Task ChangeStatusAsync_ShouldClearSessionsAndOrders_WhenStaffSetsTableToEmpty()
    {
        // Arrange
        var table = new Table
        {
            Label = "STAFF-CLEAR",
            PlaceId = 1,
            Status = TableStatus.occupied,
            QrSalt = "staff-clear",
            Width = 100,
            Height = 100,
            X = 50,
            Y = 50
        };
        await _tableRepo.AddAsync(table);

        var guest = new GuestSession { TableId = table.Id, Token = "guest-to-end" };
        await _guestSessionRepo.AddAsync(guest);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        // Act
        await _service.ChangeStatusAsync("staff-clear", TableStatus.empty);

        // Assert
        var updated = await _tableRepo.GetByIdAsync(table.Id);
        Assert.That(updated!.Status, Is.EqualTo(TableStatus.empty));

        var sessions = await _guestSessionRepo.GetFilteredAsync(
            filterBy: g => g.TableId == table.Id && g.IsValid);
        Assert.That(sessions, Is.Empty);
    }

    [Test]
    public async Task ChangeStatusAsync_ShouldUpdateStatus_WhenStaffSetsNonEmptyStatus()
    {
        // Arrange
        var table = new Table
        {
            Label = "STAFF-SET",
            PlaceId = 1,
            Status = TableStatus.occupied,
            QrSalt = "staff-set",
            Width = 100,
            Height = 100,
            X = 50,
            Y = 50
        };
        await _tableRepo.AddAsync(table);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        // Act
        await _service.ChangeStatusAsync("staff-set", TableStatus.reserved);

        // Assert
        var updated = await _tableRepo.GetByIdAsync(table.Id);
        Assert.That(updated!.Status, Is.EqualTo(TableStatus.reserved));
    }

    [Test]
    public async Task ChangeStatusAsync_ShouldFail_WhenStaffFromWrongPlace()
    {
        // Arrange
        var table = new Table
        {
            Label = "STAFF-WRONG",
            PlaceId = 2, // belongs to different place
            Status = TableStatus.occupied,
            QrSalt = "wrong-place",
            Width = 100,
            Height = 100,
            X = 50,
            Y = 50
        };
        await _tableRepo.AddAsync(table);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1)); // unauthorized

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedBusinessAccessException>(() =>
            _service.ChangeStatusAsync("wrong-place", TableStatus.empty));

        Assert.That(ex, Is.Not.Null);
    }
}
