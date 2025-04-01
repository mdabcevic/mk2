
using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BartenderTests;

[TestFixture]
public class TableInteractionServiceTests
{
    private IMapper _mapper;
    private IRepository<Tables> _tableRepo;
    private IGuestSessionService _guestSession;
    private ITableSessionService _tableSession;
    private ILogger<TableInteractionService> _logger;
    private ICurrentUserContext _userContext;
    private TableInteractionService _service;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Tables, TableScanDto>();
            cfg.CreateMap<Tables, TableDto>();
        });

        _mapper = config.CreateMapper();
        _tableRepo = Substitute.For<IRepository<Tables>>();
        _guestSession = Substitute.For<IGuestSessionService>();
        _tableSession = Substitute.For<ITableSessionService>();
        _logger = Substitute.For<ILogger<TableInteractionService>>();
        _userContext = Substitute.For<ICurrentUserContext>();
        _service = new TableInteractionService(_tableRepo, _guestSession, _tableSession, _logger, _userContext, _mapper);
    }

    private static Tables CreateTable(string salt, TableStatus status = TableStatus.occupied)
    {
        return new Tables
        {
            Id = 1,
            Label = "1",
            PlaceId = 1,
            QrSalt = salt,
            Status = status,
            IsDisabled = false
        };
    }

    private static Staff CreateStaff(int id = 1, int placeId = 1, EmployeeRole role = EmployeeRole.manager)
    {
        return new Staff
        {
            Id = id,
            PlaceId = placeId,
            OIB = "12345678911",
            Role = role,
            FullName = "Test User",
            Username = "testusername",
            Password = "hashed"
        };
    }

    [Test]
    public async Task GetBySaltAsync_ReturnsNotFound_IfTableDoesNotExist()
    {
        // Arrange
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>())
            .Returns((Tables?)null);

        // Act
        var result = await _service.GetBySaltAsync("invalid_salt");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Data, Is.Null);
        });
        await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Tables>());
        //await _sessionRepo.DidNotReceive().GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>());
    }

    [Test]
    public async Task GetBySaltAsync_ReturnsUnauthorized_IfGuestScansDisabledTable()
    {
        // Arrange
        var table = new Tables { Id = 1, QrSalt = "salt", IsDisabled = true };
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
        _userContext.IsGuest.Returns(true);

        // Act
        var result = await _service.GetBySaltAsync("salt");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Data, Is.Null); //TODO: should reroute to menu
        });

        await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Tables>());
        //await _sessionRepo.DidNotReceive().AddAsync(Arg.Any<GuestSession>());
    }

    [Test]
    public async Task GetBySaltAsync_StaffCanScanDisabledTable_AndMarkAsOccupied()
    {
        // Arrange
        var table = CreateTable("salt", TableStatus.empty);
        table.IsDisabled = true;
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
        _userContext.IsGuest.Returns(false);

        // Act
        var result = await _service.GetBySaltAsync("salt");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.TypeOf<TableScanDto>());
            Assert.That(result.Data!.GuestToken, Is.Empty); // no token for staff
        });

        await _tableRepo.Received().UpdateAsync(table);
        //await _sessionRepo.DidNotReceive().AddAsync(Arg.Any<GuestSession>());
    }

    [Test]
    public async Task GetBySaltAsync_ReturnsConflict_IfActiveSessionExistsFromAnotherUser()
    {
        // Arrange
        var table = new Tables { Id = 1, QrSalt = "salt", IsDisabled = false, Status = TableStatus.occupied };
        var activeSession = new GuestSession
        {
            TableId = 1,
            Token = "existing.token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        _userContext.IsGuest.Returns(true);
        _userContext.GetRawToken().Returns("another.user.token"); // not matching
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
        //_sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(activeSession);

        // Act
        var result = await _service.GetBySaltAsync("salt");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
        });

        // Ensure session was not updated/added
        //await _sessionRepo.DidNotReceive().AddAsync(Arg.Any<GuestSession>());
    }

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
    public async Task GetBySaltAsync_ShouldCreateSessionAndReturnToken_WhenAllValid()
    {
        // Arrange
        var table = new Tables { Id = 1, QrSalt = "salt123", Status = TableStatus.empty };
        _userContext.IsGuest.Returns(true);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
        //_jwtService.GenerateGuestToken(Arg.Any<int>(), Arg.Any<Guid>(), Arg.Any<DateTime>()).Returns("generated.token");

        // Act
        var result = await _service.GetBySaltAsync("salt123");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.TypeOf<TableScanDto>());
            Assert.That(result.Data!.GuestToken, Is.EqualTo("generated.token"));
            Assert.That(table.Status, Is.EqualTo(TableStatus.occupied));
        });
        await _tableRepo.Received(1).UpdateAsync(table);
        //await _sessionRepo.Received(1).AddAsync(Arg.Any<GuestSession>());
        //await _sessionRepo.DidNotReceive().GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>());
    }

    [Test]
    public async Task ChangeStatusAsync_GuestCanFreeTableWithValidSession()
    {
        // Arrange
        var table = CreateTable("valid");
        var token = "guest-token";
        var session = new GuestSession
        {
            TableId = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>())
            .Returns(table);
        _userContext.IsGuest.Returns(true);
        _userContext.GetRawToken().Returns(token);
        //_sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        // Act
        var result = await _service.ChangeStatusAsync("valid", TableStatus.empty);

        // Assert
        Assert.Multiple(() =>
        {

            Assert.That(result.Success, Is.True);
            Assert.That(table.Status, Is.EqualTo(TableStatus.empty));
        });
        //await _sessionRepo.Received().DeleteAsync(session);
        await _tableRepo.Received().UpdateAsync(table);
    }

    [Test]
    public async Task ChangeStatusAsync_GuestFailsWithExpiredSession()
    {
        // Arrange
        var table = CreateTable("expired");
        var token = "guest-token";
        var session = new GuestSession
        {
            TableId = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>())
            .Returns(table);
        _userContext.IsGuest.Returns(true);
        _userContext.GetRawToken().Returns(token);
        //_sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        // Act
        var result = await _service.ChangeStatusAsync("expired", TableStatus.empty);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        //await _sessionRepo.DidNotReceive().DeleteAsync(Arg.Any<GuestSession>());
        await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Tables>());
    }

    [Test]
    public async Task ChangeStatusAsync_GuestFreesAlreadyEmptyTableShouldSucceed()
    {
        // Arrange
        var table = CreateTable("noop", TableStatus.empty);
        var token = "guest-token";
        var session = new GuestSession
        {
            TableId = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>())
            .Returns(table);
        _userContext.IsGuest.Returns(true);
        _userContext.GetRawToken().Returns(token);
        //_sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        // Act
        var result = await _service.ChangeStatusAsync("noop", TableStatus.empty);

        // Assert
        Assert.That(result.Success, Is.True);
        //await _sessionRepo.DidNotReceive().DeleteAsync(session);
        await _tableRepo.DidNotReceive().UpdateAsync(table); // because it's already empty
    }

    [Test]
    public async Task ChangeStatusAsync_GuestFailsWhenTryingToSetNonEmptyStatus()
    {
        // Arrange
        var table = CreateTable("bad");
        var token = "guest-token";
        var session = new GuestSession
        {
            TableId = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>())
            .Returns(table);
        _userContext.IsGuest.Returns(true);
        _userContext.GetRawToken().Returns(token);
        //_sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        // Act
        var result = await _service.ChangeStatusAsync("bad", TableStatus.reserved);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _tableRepo.DidNotReceive().UpdateAsync(table);
        //await _sessionRepo.DidNotReceive().UpdateAsync(session);
    }

    [Test]
    public async Task ChangeStatusAsync_StaffCanChangeStatusIfAuthorized()
    {
        // Arrange
        var table = CreateTable("testtabletoken");
        var user = CreateStaff(1);

        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>())
            .Returns(table);
        _userContext.IsGuest.Returns(false);
        _userContext.GetCurrentUserAsync().Returns(user);

        // Act
        var result = await _service.ChangeStatusAsync("testtabletoken", TableStatus.reserved);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(table.Status, Is.EqualTo(TableStatus.reserved));
        });
        await _tableRepo.Received().UpdateAsync(table);
        //await _sessionRepo.DidNotReceive().AddAsync(Arg.Any<GuestSession>());
    }

    [Test]
    public async Task ChangeStatusAsync_StaffFailsWhenFromOtherPlace()
    {
        // Arrange
        var table = CreateTable("wrong-place");
        var user = CreateStaff(10, 99);

        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>())
            .Returns(table);
        _userContext.IsGuest.Returns(false);
        _userContext.GetCurrentUserAsync().Returns(user);

        // Act
        var result = await _service.ChangeStatusAsync("wrong-place", TableStatus.empty);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(table.Status, Is.Not.EqualTo(TableStatus.empty));
        });
    }
}
