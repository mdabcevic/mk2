using Bartender.Data.Models;
using Bartender.Domain;
using NSubstitute;
using System.Linq.Expressions;
using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Microsoft.Extensions.Logging;
using Bartender.Domain.DTO.Table;

namespace BartenderTests;

[TestFixture]
public class TableManagementServiceTests
{
    private IMapper _mapper;
    private IRepository<Tables> _tableRepo;
    private ILogger<TableInteractionService> _logger;
    private ICurrentUserContext _userContext;
    private TableManagementService _service;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Tables, TableDto>();
            cfg.CreateMap<UpsertTableDto, Tables>();
        });

        _mapper = config.CreateMapper();
        _tableRepo = Substitute.For<IRepository<Tables>>();
        _logger = Substitute.For<ILogger<TableInteractionService>>();
        _userContext = Substitute.For<ICurrentUserContext>();

        _service = new TableManagementService(_tableRepo, _logger, _userContext, _mapper);
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
    public async Task GetAllAsync_ShouldReturnOnlyTablesFromUsersPlace()
    {
        // Arrange
        var user = CreateStaff(placeId: 1);
        var allTables = new List<Tables>
    {
        new() { Id = 1, Label = "T1", PlaceId = 1 },
        new() { Id = 2, Label = "T2", PlaceId = 1 },
        new() { Id = 3, Label = "T3", PlaceId = 99 } // belongs to another place
    };

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetAllAsync().Returns(allTables);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data?.Count, Is.EqualTo(2));
            Assert.That(result.Data!.All(t => t.Label is "T1" or "T2"));
        });
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
