
using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Bartender.Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Bartender.Domain.DTO;
using System.Linq.Expressions;
using NSubstitute.Core.Arguments;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BartenderTests;

[TestFixture]
public class TableServiceTests
{
    private IMapper _mapper;
    private IRepository<Tables> _tableRepo;
    private IRepository<GuestSession> _sessionRepo;
    private ILogger<TableService> _logger;
    private IJwtService _jwtService;
    private ICurrentUserContext _userContext;
    private TableService _service;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Tables, TableScanDto>();
            cfg.CreateMap<Tables, TableDto>();
            cfg.CreateMap<UpsertTableDto, Tables>();
        });

        _mapper = config.CreateMapper();
        _tableRepo = Substitute.For<IRepository<Tables>>();
        _sessionRepo = Substitute.For<IRepository<GuestSession>>();
        _logger = Substitute.For<ILogger<TableService>>();
        _jwtService = Substitute.For<IJwtService>();
        _userContext = Substitute.For<ICurrentUserContext>();
        _userContext.IsGuest.Returns(true);

        _service = new TableService(_tableRepo, _sessionRepo, _logger, _jwtService, _userContext, _mapper);
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

    // TESTS BELOW

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
        await _sessionRepo.DidNotReceive().GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>());
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
        await _sessionRepo.DidNotReceive().AddAsync(Arg.Any<GuestSession>());
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
        await _sessionRepo.DidNotReceive().AddAsync(Arg.Any<GuestSession>());
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
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(activeSession);

        // Act
        var result = await _service.GetBySaltAsync("salt");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
        });

        // Ensure session was not updated/added
        await _sessionRepo.DidNotReceive().AddAsync(Arg.Any<GuestSession>());
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
        _jwtService.GenerateGuestToken(Arg.Any<int>(), Arg.Any<Guid>(), Arg.Any<DateTime>()).Returns("generated.token");

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
        await _sessionRepo.Received(1).AddAsync(Arg.Any<GuestSession>());
        await _sessionRepo.DidNotReceive().GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>());
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
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>())
            .Returns(session);

        // Act
        var result = await _service.ChangeStatusAsync("valid", TableStatus.empty);

        // Assert
        Assert.Multiple(() =>
        {
            
            Assert.That(result.Success, Is.True);
            Assert.That(table.Status, Is.EqualTo(TableStatus.empty));
        });
        await _sessionRepo.Received().DeleteAsync(session);
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
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>())
            .Returns(session);

        // Act
        var result = await _service.ChangeStatusAsync("expired", TableStatus.empty);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _sessionRepo.DidNotReceive().DeleteAsync(Arg.Any<GuestSession>());
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
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>())
            .Returns(session);

        // Act
        var result = await _service.ChangeStatusAsync("noop", TableStatus.empty);

        // Assert
        Assert.That(result.Success, Is.True);
        await _sessionRepo.DidNotReceive().DeleteAsync(session);
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
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>())
            .Returns(session);

        // Act
        var result = await _service.ChangeStatusAsync("bad", TableStatus.reserved);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _tableRepo.DidNotReceive().UpdateAsync(table);
        await _sessionRepo.DidNotReceive().UpdateAsync(session);
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
        await _sessionRepo.DidNotReceive().AddAsync(Arg.Any<GuestSession>());
    }

    [Test]
    public async Task ChangeStatusAsync_StaffFailsWhenFromOtherPlace()
    {
        // Arrange
        var table = CreateTable("wrong-place");
        var user = CreateStaff(10,99);

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

    [Test]
    public async Task AddAsync_ShouldAddTable_WhenLabelIsUnique()
    {
        // Arrange
        var user = CreateStaff();
        var dto = new UpsertTableDto { Label = "T1", Seats = 4 };

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.ExistsAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(false);

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _tableRepo.Received().AddAsync(Arg.Is<Tables>(t =>
            t.PlaceId == user.PlaceId &&
            t.Label == dto.Label &&
            t.Seats == dto.Seats &&
            t.Status == TableStatus.empty &&
            !string.IsNullOrWhiteSpace(t.QrSalt)
        ));
    }

    [Test]
    public async Task AddAsync_ShouldFail_WhenLabelAlreadyExists()
    {
        // Arrange
        var user = CreateStaff();
        var dto = new UpsertTableDto { Label = "T1", Seats = 2 };

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.ExistsAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(true);

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
        });
        await _tableRepo.DidNotReceive().AddAsync(Arg.Any<Tables>());
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateTable_WhenAuthorized()
    {
        // Arrange
        var user = CreateStaff();
        var dto = new UpsertTableDto { Label = "1", Seats = 6 };
        var existing = CreateTable("some-token");
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(existing);

        // Act
        var result = await _service.UpdateAsync("T1", dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(existing.Seats, Is.EqualTo(6));
        });
        await _tableRepo.Received().UpdateAsync(existing);
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenTableNotFound()
    {
        // Arrange
        var user = CreateStaff();
        var dto = new UpsertTableDto { Label = "T1", Seats = 6 };

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns((Tables?)null);

        // Act
        var result = await _service.UpdateAsync("T1", dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
        await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Tables>());
    }

    [Test]
    public async Task DeleteAsync_ShouldDeleteTable_WhenAuthorized()
    {
        // Arrange
        var user = CreateStaff();
        var table = CreateTable("qr-code");
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
        
        // Act
        var result = await _service.DeleteAsync("T1");

        // Assert
        Assert.That(result.Success, Is.True);
        await _tableRepo.Received().DeleteAsync(table);
    }

    [Test]
    public async Task DeleteAsync_ShouldFail_WhenTableNotFound()
    {
        // Arrange
        var user = CreateStaff();
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns((Tables?)null);

        // Act
        var result = await _service.DeleteAsync("missing");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
        await _tableRepo.DidNotReceive().DeleteAsync(Arg.Any<Tables>());
    }

    [Test]
    public async Task RegenerateSaltAsync_ShouldGenerateNewSalt_WhenTableFound()
    {
        // Arrange
        var user = CreateStaff();
        var table = CreateTable("old-salt");
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);

        // Act
        var result = await _service.RegenerateSaltAsync("T1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(table.QrSalt, Is.Not.EqualTo("old-salt"));
        });

        await _tableRepo.Received().UpdateAsync(table);
    }

    [Test]
    public async Task RegenerateSaltAsync_ShouldFail_WhenTableNotFound()
    {
        // Arrange
        var user = CreateStaff();
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns((Tables?)null);

        // Act
        var result = await _service.RegenerateSaltAsync("missing");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
        await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Tables>());
    }

    [Test]
    public async Task SwitchDisabledAsync_ShouldUpdateFlag_WhenAuthorized()
    {
        // Arrange
        var user = CreateStaff();
        var table = CreateTable("some-code");
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);

        // Act
        var result = await _service.SwitchDisabledAsync("1", true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(table.IsDisabled, Is.True);
        });
        await _tableRepo.Received().UpdateAsync(table);
    }

    [Test]
    public async Task SwitchDisabledAsync_ShouldFail_WhenTableNotFound()
    {
        // Arrange
        var user = CreateStaff();
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns((Tables?)null);

        // Act
        var result = await _service.SwitchDisabledAsync("T404", false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
        await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Tables>());
    }

    [Test]
    public async Task GetByLabelAsync_ShouldReturnTable_WhenExists()
    {
        // Arrange
        var user = CreateStaff();
        var table = CreateTable("qr-xyz");
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);

        // Act
        var result = await _service.GetByLabelAsync("1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.TypeOf<TableDto>());
            Assert.That(result.Data?.Label, Is.EqualTo("1"));
        });
        await _tableRepo.Received(1).GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>());
    }

    [Test]
    public async Task GetByLabelAsync_ShouldFail_WhenTableMissing()
    {
        // Arrange
        var user = CreateStaff();
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns((Tables?)null);

        // Act
        var result = await _service.GetByLabelAsync("Missing");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }
}
