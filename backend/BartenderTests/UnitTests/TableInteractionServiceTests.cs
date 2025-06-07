using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Table;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;

namespace BartenderTests.UnitTests;

[TestFixture]
public class TableInteractionServiceTests
{
    private IMapper _mapper;
    private IRepository<Table> _tableRepo;
    private IGuestSessionService _guestSession;
    private ITableSessionService _tableSession;
    private IOrderRepository _orderRepo;
    private ILogger<TableInteractionService> _logger;
    private ICurrentUserContext _userContext;
    private TableInteractionService _service;
    private INotificationService _notificationService;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Table, TableScanDto>();
            cfg.CreateMap<Table, TableDto>();
        });

        _mapper = config.CreateMapper();
        _tableRepo = Substitute.For<IRepository<Table>>();
        _guestSession = Substitute.For<IGuestSessionService>();
        _tableSession = Substitute.For<ITableSessionService>();
        _orderRepo = Substitute.For<IOrderRepository>();
        _logger = Substitute.For<ILogger<TableInteractionService>>();
        _userContext = Substitute.For<ICurrentUserContext>();
        _notificationService = Substitute.For<INotificationService>();
        _service = new TableInteractionService(_tableRepo, _guestSession, _tableSession, _orderRepo, _notificationService, _logger, _userContext, _mapper);
    }

    [Test]
    public async Task GetBySaltAsync_Should_Throw_NotFoundException_If_Table_Not_Exists()
    {
        // Arrange
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Table, bool>>>()).Returns((Table?)null);  // Simulate table not found

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetBySaltAsync("salt"));
        Assert.That(ex.Message, Is.EqualTo("Invalid QR code"));  // Adjust the message based on your exception's message

        // Ensure no repository update or session creation occurs
        await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Table>());
        await _guestSession.DidNotReceive().CreateSessionAsync(Arg.Any<int>(), "passphrase");
    }

    [Test]
    public async Task GetBySaltAsync_Should_Throw_UnauthorizedAccessException_If_Guest_Scans_Disabled_Table()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 1, salt: "salt", disabled: true);  // Create a disabled table
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Table, bool>>>()).Returns(table);  // Simulate table retrieval
        _userContext.IsGuest.Returns(true);  // Simulate the current user as a guest

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.GetBySaltAsync("salt"));
        Assert.That(ex.Message, Is.EqualTo("QR for this table is currently unavailable. Waiter is coming."));  // Adjust the message based on your exception's message

        // Ensure no repository update or session creation occurs
        await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Table>());
        await _guestSession.DidNotReceive().CreateSessionAsync(Arg.Any<int>(), "passphrase");
    }

    [Test]
    public async Task GetBySaltAsync_StaffCanScanDisabledTable_AndMarkAsOccupied()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 1, salt: "salt", status: TableStatus.empty, disabled: true);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Table, bool>>>()).Returns(table);
        _userContext.IsGuest.Returns(false);  // Simulate the current user as staff (not a guest)

        // Act
        var result = await _service.GetBySaltAsync("salt");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);  // Ensure result is not null
            Assert.That(result, Is.InstanceOf<TableScanDto>());  // Ensure result is of type TableScanDto
            Assert.That(result.Label, Is.EqualTo(table.Label));  // Check the table label
            //Assert.That(result.IsSessionEstablished, Is.True);
            Assert.That(table.Status, Is.EqualTo(TableStatus.occupied));  // Check that the table status is updated to 'occupied'
        });

        await _tableRepo.Received(1).UpdateAsync(table);
        await _guestSession.DidNotReceive().CreateSessionAsync(Arg.Any<int>(), "passphrase");
    }


    //[Test]
    //public async Task GetBySaltAsync_ReturnsConflict_IfActiveSessionExistsFromAnotherUser()
    //{
    //    // Arrange
    //    var table = new Tables { Id = 1, QrSalt = "salt", IsDisabled = false, Status = TableStatus.occupied };

    //    _userContext.IsGuest.Returns(true);
    //    _userContext.GetRawToken().Returns("another.user.token");

    //    _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);

    //    _tableSession.HasActiveSessionAsync(table.Id, _userContext.GetRawToken()!).Returns(true);
    //    _tableSession.GetConflictingSessionAsync("another.user.token", table.Id).Returns(Arg.Any<GuestSession>());

    //    // Act
    //    var result = await _service.GetBySaltAsync("salt");

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.False);
    //        Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
    //        Assert.That(result.Data, Is.Null);
    //    });

    //    await _guestSession.DidNotReceive().CreateSessionAsync(Arg.Any<int>());
    //    await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Tables>());
    //}

    //[Test]
    //public async Task GetBySaltAsync_AllowsScan_WhenTokenMatchesActiveSession()
    //{
    //    // Arrange
    //    var table = new Tables { Id = 1, QrSalt = "salt", IsDisabled = false, Status = TableStatus.occupied };
    //    var matchingToken = "active.jwt.token";

    //    var activeSession = new GuestSession
    //    {
    //        TableId = 1,
    //        Token = matchingToken,
    //        ExpiresAt = DateTime.UtcNow.AddMinutes(10)
    //    };

    //    _userContext.IsGuest.Returns(true);
    //    _userContext.GetRawToken().Returns(matchingToken); // matches token on session
    //    _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
    //    _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(activeSession);

    //    // Act
    //    var result = await _service.GetBySaltAsync("salt");

    //    Assert.Multiple(() =>
    //    {
    //        // Assert
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(result.Data, Is.Not.Null);
    //        Assert.That(result.Data?.GuestToken, Is.EqualTo(matchingToken));
    //    });

    //    // No new session is created
    //    await _sessionRepo.DidNotReceive().AddAsync(Arg.Any<GuestSession>());
    //}

    //[Test]
    //public async Task GetBySaltAsync_ShouldResumeExpiredSession_WhenTokenMatches()
    //{
    //    // Arrange
    //    var table = new Tables { Id = 1, QrSalt = "salt123", Status = TableStatus.occupied };
    //    var expiredToken = "expired.jwt.token";
    //    var expiredSession = new GuestSession
    //    {
    //        TableId = 1,
    //        Token = expiredToken,
    //        ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
    //    };

    //    _userContext.IsGuest.Returns(true);
    //    _userContext.GetRawToken().Returns(expiredToken);
    //    _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
    //    _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns((GuestSession?)null);
    //    _sessionRepo.Query().Returns(new List<GuestSession> { expiredSession }.AsQueryable());

    //    // Act
    //    var result = await _service.GetBySaltAsync("salt123");

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(result.Data, Is.Not.Null);
    //    });
    //}

    [Test]
    public async Task GetBySaltAsync_Should_Create_Session_When_All_Valid()
    {
        // Arrange
        var table = new Table { Id = 1, QrSalt = "salt123", Status = TableStatus.empty };
        _userContext.IsGuest.Returns(true);  // Simulate that the current user is a guest
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Table, bool>>>()).Returns(table);  // Simulate retrieving the table by QR salt
        _guestSession.CreateSessionAsync(table.Id, Arg.Any<string>()).Returns("generated.token");  // Simulate session creation and return a token

        // Act
        var result = await _service.GetBySaltAsync("salt123");  // Call the service method

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<TableScanDto>());
            Assert.That(result.IsSessionEstablished, Is.True);
            Assert.That(table.Status, Is.EqualTo(TableStatus.occupied));
        });

        await _tableRepo.Received(1).UpdateAsync(table);
        await _guestSession.Received(1).CreateSessionAsync(table.Id, Arg.Any<string>());
    }

    [Test]
    public async Task ChangeStatusAsync_GuestCanFreeTableWithValidSession()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 1, label: "1", salt: "qrsalt", status: TableStatus.occupied);
        var session = TestDataFactory.CreateValidGuestSession(table, token: "guest-token");
        var token = "guest-token";

        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Table, bool>>>()).Returns(table);  // Simulate getting the table
        _userContext.IsGuest.Returns(true);  // Simulate that the current user is a guest
        _userContext.GetRawToken().Returns(token);  // Simulate the guest's token
        _guestSession.GetByTokenAsync(table.Id, token).Returns(session);  // Simulate retrieving the session for the guest
        _tableSession.HasActiveSessionAsync(table.Id, token).Returns(true);  // Simulate that the session is active

        // Act
        await _service.ChangeStatusAsync("qrsalt", TableStatus.empty);  // Change the table status to 'empty'

        // Assert
        Assert.That(table.Status, Is.EqualTo(TableStatus.empty));
        await _tableRepo.Received(1).UpdateAsync(table);
    }

    //[Test]
    //public async Task ChangeStatusAsync_GuestFailsWithExpiredSession()
    //{
    //    // Arrange
    //    var table = TestDataFactory.CreateValidTable(id: 1, label: "1");
    //    var token = "guest-token";
    //    var expiredSession = new GuestSession
    //    {
    //        Id = Guid.NewGuid(),
    //        TableId = table.Id,
    //        Token = token,
    //        ExpiresAt = DateTime.UtcNow.AddMinutes(-10)
    //    };

    //    _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
    //    _userContext.IsGuest.Returns(true);
    //    _userContext.GetRawToken().Returns(token);
    //    _guestSession.GetByTokenAsync(table.Id, token).Returns(expiredSession);

    //    // Act
    //    var result = await _service.ChangeStatusAsync("expired", TableStatus.empty);

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.False);
    //        Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
    //        Assert.That(table.Status, Is.EqualTo(TableStatus.occupied));
    //    });

    //    await _guestSession.DidNotReceive().DeleteSessionAsync(Arg.Any<Guid>());
    //    await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Tables>());
    //}

    [Test]
    public async Task ChangeStatusAsync_GuestFreesAlreadyEmptyTable_Should_Succeed()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 1, label: "1", salt: "qrsalt", status: TableStatus.empty);  // Table is already empty
        var token = "guest-token";

        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Table, bool>>>()).Returns(table);  // Simulate retrieving the table by QR salt
        _userContext.IsGuest.Returns(true);  // Simulate that the current user is a guest
        _userContext.GetRawToken().Returns(token);  // Simulate the guest's token
        _tableSession.HasActiveSessionAsync(table.Id, token).Returns(true);  // Simulate that the session is active

        // Act
        await _service.ChangeStatusAsync("qrsalt", TableStatus.empty);  // Attempt to change the table status to 'empty' again

        // Assert
        Assert.That(table.Status, Is.EqualTo(TableStatus.empty));  // Ensure the table status remains 'empty'
        await _tableRepo.DidNotReceive().UpdateAsync(table);
    }

    [Test]
    public async Task ChangeStatusAsync_GuestFailsWhenTryingToSetNonEmptyStatus()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 1, label: "1", status: TableStatus.empty);  // Table is initially empty
        var token = "guest-token";
        var session = TestDataFactory.CreateValidGuestSession(table, token: token);

        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Table, bool>>>()).Returns(table);  // Simulate getting the table
        _userContext.IsGuest.Returns(true);  // Simulate the current user is a guest
        _userContext.GetRawToken().Returns(token);  // Simulate the guest's token
        _guestSession.GetByTokenAsync(table.Id, token).Returns(session);  // Simulate retrieving the guest session

        // Act & Assert
        var ex = Assert.ThrowsAsync<AuthorizationException>(async () => await _service.ChangeStatusAsync("1", TableStatus.reserved));  // Expect an exception
        Assert.That(ex.Message, Does.Contain("Unauthorized"));  // Adjust the message based on your exception

        // Ensure that no updates or session deletions occur
        await _tableRepo.DidNotReceive().UpdateAsync(table);
        await _guestSession.DidNotReceive().DeleteSessionAsync(session.Id);
    }

    [Test]
    public async Task ChangeStatusAsync_StaffCanChangeStatusIfAuthorized()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 1, placeid: 1, label: "1", status: TableStatus.empty);
        var user = TestDataFactory.CreateValidStaff(placeid: 1, role: EmployeeRole.regular);  // Staff with correct placeId and role

        _userContext.IsGuest.Returns(false);  // Simulate the current user is not a guest
        _userContext.GetCurrentUserAsync().Returns(user);  // Simulate getting the current staff user
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Table, bool>>>()).Returns(table);  // Simulate retrieving the table by ID

        // Act
        await _service.ChangeStatusAsync("1", TableStatus.reserved);  // Change table status to 'reserved'

        // Assert
        Assert.That(table.Status, Is.EqualTo(TableStatus.reserved));  // Ensure the table's status is updated to 'reserved'
        await _tableRepo.Received(1).UpdateAsync(table);  // Verify that UpdateAsync was called to update the table's status
    }

    [Test]
    public async Task ChangeStatusAsync_StaffFailsWhenFromOtherPlace()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 100, placeid: 99, label: "1", status: TableStatus.occupied);  // Table from a different place
        var user = TestDataFactory.CreateValidStaff(placeid: 50, role: EmployeeRole.regular);  // Staff from a different place

        _userContext.IsGuest.Returns(false);  // Simulate the current user is not a guest
        _userContext.GetCurrentUserAsync().Returns(user);  // Simulate getting the current staff user
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Table, bool>>>()).Returns(table);  // Simulate retrieving the table by QR salt

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedBusinessAccessException>(async () => await _service.ChangeStatusAsync("1", TableStatus.empty));  // Expect an exception
        Assert.That(ex.Message, Does.Contain($"Access"));
        Assert.That(ex.Message, Does.Contain($"denied"));
        Assert.That(table.Status, Is.EqualTo(TableStatus.occupied));  // The table's status should remain 'occupied'
        await _tableRepo.DidNotReceive().UpdateAsync(table);
    }

    [Test]
    public async Task ChangeStatusAsync_Should_Throw_TableNotFoundException_When_Table_Does_Not_Exist()
    {
        // Arrange
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Table, bool>>>()).Returns((Table?)null);  // Simulate table not found

        // Act & Assert
        var ex = Assert.ThrowsAsync<TableNotFoundException>(async () => await _service.ChangeStatusAsync("missing-token", TableStatus.empty));  // Expect TableNotFoundException
        Assert.That(ex.Message, Does.Contain("not found"));  // Adjust the message based on your exception's message

        // Ensure no update is performed on the repository
        await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Table>());
    }

    //[Test]
    //public async Task GetBySaltAsync_ShouldResumeActiveSession_WhenTokenMatches()
    //{
    //    // Arrange
    //    var table = CreateTable("salt", TableStatus.occupied);
    //    var token = "active.jwt.token";

    //    _userContext.IsGuest.Returns(true);
    //    _userContext.GetRawToken().Returns(token);

    //    _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
    //    _tableSession.HasActiveSessionAsync(table.Id).Returns(true);
    //    _tableSession.IsSameTokenAsActiveAsync(table.Id, token).Returns(true);

    //    // Act
    //    var result = await _service.GetBySaltAsync("salt");

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(result.Data, Is.Not.Null);
    //        Assert.That(result.Data?.GuestToken, Is.EqualTo(token));
    //    });

    //    await _guestSession.DidNotReceive().CreateSessionAsync(table.Id);
    //    await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Tables>());
    //}

    //[Test]
    //public async Task GetBySaltAsync_ShouldResumeExpiredSession_WhenTokenMatches()
    //{
    //    // Arrange
    //    var table = CreateTable("salt123", TableStatus.occupied);
    //    var token = "expired.jwt.token";

    //    _userContext.IsGuest.Returns(true);
    //    _userContext.GetRawToken().Returns(token);

    //    _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
    //    _tableSession.HasActiveSessionAsync(table.Id).Returns(false);
    //    _tableSession.CanResumeExpiredSessionAsync(table.Id, token).Returns(true);

    //    _guestSession.CreateSessionAsync(table.Id).Returns("new.token");

    //    // Act
    //    var result = await _service.GetBySaltAsync("salt123");

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(result.Data, Is.Not.Null);
    //        Assert.That(result.Data!.GuestToken, Is.EqualTo("new.token"));
    //        Assert.That(table.Status, Is.EqualTo(TableStatus.occupied));
    //    });

    //    await _tableRepo.Received().UpdateAsync(table);
    //}

    //[Test]
    //public async Task GetBySaltAsync_ShouldFail_WhenTokenCannotResumeExpiredSession()
    //{
    //    // Arrange
    //    var table = CreateTable("stale", TableStatus.occupied);
    //    var token = "wrong.expired.token";

    //    _userContext.IsGuest.Returns(true);
    //    _userContext.GetRawToken().Returns(token);

    //    _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
    //    _tableSession.HasActiveSessionAsync(table.Id).Returns(false);
    //    _tableSession.CanResumeExpiredSessionAsync(table.Id, token).Returns(false);

    //    // Act
    //    var result = await _service.GetBySaltAsync("stale");

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.False);
    //        Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
    //        Assert.That(result.Data, Is.Null);
    //    });

    //    await _guestSession.DidNotReceive().CreateSessionAsync(Arg.Any<int>());
    //    await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Tables>());
    //}
}
