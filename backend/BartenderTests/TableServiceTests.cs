
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

    private Tables CreateTable(string salt, TableStatus status = TableStatus.occupied)
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

    private Staff CreateStaff(int id = 1, int placeId = 1, EmployeeRole role = EmployeeRole.manager)
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
    public async Task ReturnsNotFound_IfTableDoesNotExist()
    {
        _tableRepo.GetByKeyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Tables, bool>>>())
            .Returns((Tables?)null);

        var result = await _service.GetBySaltAsync("invalid_salt");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task ReturnsUnauthorized_IfTableIsDisabled()
    {
        _tableRepo.GetByKeyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Tables, bool>>>())
            .Returns(new Tables { Id = 1, QrSalt = "salt", IsDisabled = true });

        var result = await _service.GetBySaltAsync("salt");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
    }

    [Test]
    public async Task ReturnsConflict_IfActiveSessionExists()
    {
        var table = new Tables { Id = 1, QrSalt = "salt", IsDisabled = false, Status = TableStatus.occupied };

        _tableRepo.GetByKeyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Tables, bool>>>())
            .Returns(table);

        _sessionRepo.GetByKeyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<GuestSession, bool>>>())
            .Returns(new GuestSession { TableId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(5) });

        var result = await _service.GetBySaltAsync("salt");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
        });
    }

    [Test]
    public async Task GetBySaltAsync_ShouldResumeExpiredSession_WhenTokenMatches()
    {
        // Arrange
        var table = new Tables { Id = 1, QrSalt = "salt123", Status = TableStatus.occupied };
        var expiredToken = "expired.jwt.token";
        var expiredSession = new GuestSession
        {
            TableId = 1,
            Token = expiredToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        _userContext.IsGuest.Returns(true);
        _userContext.GetRawToken().Returns(expiredToken);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns((GuestSession?)null);
        _sessionRepo.Query().Returns(new List<GuestSession> { expiredSession }.AsQueryable());

        // Act
        var result = await _service.GetBySaltAsync("salt123");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetBySaltAsync_ShouldCreateSessionAndReturnToken_WhenAllValid()
    {
        // Arrange
        var table = new Tables { Id = 1, QrSalt = "salt123", Status = TableStatus.empty };
        _userContext.IsGuest.Returns(true);
        _userContext.GetRawToken().Returns("new.jwt.token");
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns((GuestSession?)null);
        _sessionRepo.Query().Returns(new List<GuestSession>().AsQueryable());
        _jwtService.GenerateGuestToken(Arg.Any<int>(), Arg.Any<Guid>(), Arg.Any<DateTime>()).Returns("generated.token");

        // Act
        var result = await _service.GetBySaltAsync("salt123");

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data?.GuestToken, Is.EqualTo("generated.token"));
        });
    }

    [Test]
    public async Task Guest_Can_Free_Table_With_Valid_Session()
    {
        var table = CreateTable("valid");
        var token = "guest-token";
        var session = new GuestSession
        {
            TableId = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        _tableRepo.GetByKeyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Tables, bool>>>())
            .Returns(table);
        _userContext.IsGuest.Returns(true);
        _userContext.GetRawToken().Returns(token);
        _sessionRepo.GetByKeyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<GuestSession, bool>>>())
            .Returns(session);

        var result = await _service.ChangeStatusAsync("valid", TableStatus.empty);

        Assert.That(result.Success, Is.True);
        await _sessionRepo.Received().DeleteAsync(session);
        await _tableRepo.Received().UpdateAsync(table);
    }

    [Test]
    public async Task Guest_Fails_With_Expired_Session()
    {
        var table = CreateTable("expired");
        var token = "guest-token";
        var session = new GuestSession
        {
            TableId = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _tableRepo.GetByKeyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Tables, bool>>>())
            .Returns(table);
        _userContext.IsGuest.Returns(true);
        _userContext.GetRawToken().Returns(token);
        _sessionRepo.GetByKeyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<GuestSession, bool>>>())
            .Returns(session);

        var result = await _service.ChangeStatusAsync("expired", TableStatus.empty);

        Assert.That(result.Success, Is.False);
        Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
    }

    [Test]
    public async Task Guest_Frees_Already_Empty_Table_ShouldSucceed_NoOp()
    {
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

        var result = await _service.ChangeStatusAsync("noop", TableStatus.empty);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            _sessionRepo.DidNotReceive().DeleteAsync(session);
            _tableRepo.DidNotReceive().UpdateAsync(table); // because it's already empty
        });
    }

    [Test]
    public async Task Guest_Fails_When_Trying_To_Set_NonEmpty_Status()
    {
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

        var result = await _service.ChangeStatusAsync("bad", TableStatus.reserved);

        Assert.That(result.Success, Is.False);
        Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
    }

    [Test]
    public async Task Staff_Can_Change_Status_If_Authorized()
    {
        var table = CreateTable("testtabletoken");
        var user = CreateStaff(1);

        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>())
            .Returns(table);
        _userContext.IsGuest.Returns(false);
        _userContext.GetCurrentUserAsync().Returns(user);

        var result = await _service.ChangeStatusAsync("testtabletoken", TableStatus.reserved);

        Assert.That(result.Success, Is.True);
        await _tableRepo.Received().UpdateAsync(table);
    }

    [Test]
    public async Task Staff_Fails_When_From_Other_Place()
    {
        var table = CreateTable("wrong-place");
        var user = CreateStaff(10,99);

        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>())
            .Returns(table);
        _userContext.IsGuest.Returns(false);
        _userContext.GetCurrentUserAsync().Returns(user);

        var result = await _service.ChangeStatusAsync("wrong-place", TableStatus.empty);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
    }

    [Test]
    public async Task AddAsync_ShouldAddTable_WhenLabelIsUnique()
    {
        var user = CreateStaff();
        var dto = new UpsertTableDto { Label = "T1", Seats = 4 };

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.ExistsAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(false);

        var result = await _service.AddAsync(dto);

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
        var user = CreateStaff();
        var dto = new UpsertTableDto { Label = "T1", Seats = 2 };

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.ExistsAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(true);

        var result = await _service.AddAsync(dto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateTable_WhenAuthorized()
    {
        var user = CreateStaff();
        var dto = new UpsertTableDto { Label = "T1", Seats = 6 };
        var existing = CreateTable("some-token");
        existing.Label = "T1";

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(existing);

        var result = await _service.UpdateAsync("T1", dto);

        Assert.That(result.Success, Is.True);
        Assert.That(existing.Seats, Is.EqualTo(6));
        await _tableRepo.Received().UpdateAsync(existing);
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenTableNotFound()
    {
        var user = CreateStaff();
        var dto = new UpsertTableDto { Label = "T1", Seats = 6 };

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns((Tables?)null);

        var result = await _service.UpdateAsync("T1", dto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task DeleteAsync_ShouldDeleteTable_WhenAuthorized()
    {
        var user = CreateStaff();
        var table = CreateTable("qr-code");
        table.Label = "T1";

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);

        var result = await _service.DeleteAsync("T1");

        Assert.That(result.Success, Is.True);
        await _tableRepo.Received().DeleteAsync(table);
    }

    [Test]
    public async Task DeleteAsync_ShouldFail_WhenTableNotFound()
    {
        var user = CreateStaff();
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns((Tables?)null);

        var result = await _service.DeleteAsync("missing");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task RegenerateSaltAsync_ShouldGenerateNewSalt_WhenTableFound()
    {
        var user = CreateStaff();
        var table = CreateTable("old-salt");
        table.Label = "T1";

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);

        var result = await _service.RegenerateSaltAsync("T1");

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
        var user = CreateStaff();
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns((Tables?)null);

        var result = await _service.RegenerateSaltAsync("missing");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task SwitchDisabledAsync_ShouldUpdateFlag_WhenAuthorized()
    {
        var user = CreateStaff();
        var table = CreateTable("some-code");
        table.Label = "T1";

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);

        var result = await _service.SwitchDisabledAsync("T1", true);

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
        var user = CreateStaff();
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns((Tables?)null);

        var result = await _service.SwitchDisabledAsync("T404", false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task GetByLabelAsync_ShouldReturnTable_WhenExists()
    {
        var user = CreateStaff();
        var table = CreateTable("qr-xyz");
        table.Label = "T1";

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(table);

        var result = await _service.GetByLabelAsync("T1");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data?.Label, Is.EqualTo("T1"));
        });
    }

    [Test]
    public async Task GetByLabelAsync_ShouldFail_WhenTableMissing()
    {
        var user = CreateStaff();
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns((Tables?)null);

        var result = await _service.GetByLabelAsync("Missing");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }
}
