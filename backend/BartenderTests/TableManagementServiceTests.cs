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
    private ITableRepository _tableRepo;
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
        _tableRepo = Substitute.For<ITableRepository>();
        _logger = Substitute.For<ILogger<TableInteractionService>>();
        _userContext = Substitute.For<ICurrentUserContext>();

        _service = new TableManagementService(_tableRepo, _logger, _userContext, _mapper);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnOnlyTablesFromUsersPlace()
    {
        // Arrange
        var user = TestDataFactory.CreateValidStaff(placeid: 1, role: EmployeeRole.manager);
        var allTables = new List<Tables>
        {
            TestDataFactory.CreateValidTable(id: 1, label: "1"),
            TestDataFactory.CreateValidTable(id: 2, label: "2")
        };

        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetAllByPlaceAsync(user.PlaceId).Returns(allTables);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data?.Count, Is.EqualTo(2));
        });
    }

    //[Test]
    //public async Task AddAsync_ShouldFail_WhenLabelAlreadyExists()
    //{
    //    // Arrange
    //    var user = TestDataFactory.CreateValidStaff();
    //    var dto = new UpsertTableDto { Label = "T1", Seats = 2 };

    //    _userContext.GetCurrentUserAsync().Returns(user);
    //    _tableRepo.ExistsAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(true);

    //    // Act
    //    var result = await _service.AddAsync(dto);

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.False);
    //        Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
    //    });
    //    await _tableRepo.DidNotReceive().AddAsync(Arg.Any<Tables>());
    //}

    //[Test]
    //public async Task UpdateAsync_ShouldUpdateTable_WhenAuthorized()
    //{
    //    // Arrange
    //    var user = CreateStaff();
    //    var dto = new UpsertTableDto { Label = "1", Seats = 6 };
    //    var existing = CreateTable("some-token");
    //    _userContext.GetCurrentUserAsync().Returns(user);
    //    _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns(existing);

    //    // Act
    //    var result = await _service.UpdateAsync("T1", dto);

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(existing.Seats, Is.EqualTo(6));
    //    });
    //    await _tableRepo.Received().UpdateAsync(existing);
    //}

    //[Test]
    //public async Task UpdateAsync_ShouldFail_WhenTableNotFound()
    //{
    //    // Arrange
    //    var user = CreateStaff();
    //    var dto = new UpsertTableDto { Label = "T1", Seats = 6 };

    //    _userContext.GetCurrentUserAsync().Returns(user);
    //    _tableRepo.GetByKeyAsync(Arg.Any<Expression<Func<Tables, bool>>>()).Returns((Tables?)null);

    //    // Act
    //    var result = await _service.UpdateAsync("T1", dto);

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.False);
    //        Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
    //    });
    //    await _tableRepo.DidNotReceive().UpdateAsync(Arg.Any<Tables>());
    //}

    [Test]
    public async Task DeleteAsync_ShouldDeleteTable_WhenAuthorized()
    {
        // Arrange
        var user = TestDataFactory.CreateValidStaff(placeid: 1, role: EmployeeRole.manager);
        var table = TestDataFactory.CreateValidTable(placeid: 1, label: "1", salt: "qr-code");
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByPlaceLabelAsync(1, "1").Returns(table);

        // Act
        var result = await _service.DeleteAsync("1");

        // Assert
        Assert.That(result.Success, Is.True);
        await _tableRepo.Received().DeleteAsync(table);
    }

    [Test]
    public async Task DeleteAsync_ShouldFail_WhenTableNotFound()
    {
        // Arrange
        var user = TestDataFactory.CreateValidStaff();
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByPlaceLabelAsync(1, "1").Returns((Tables?)null);

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
        var user = TestDataFactory.CreateValidStaff(placeid: 1, role: EmployeeRole.manager);
        var table = TestDataFactory.CreateValidTable(placeid: 1, label: "1", salt: "old-salt");
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByPlaceLabelAsync(1, "1").Returns(table);

        // Act
        var result = await _service.RegenerateSaltAsync("1");

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
        var user = TestDataFactory.CreateValidStaff();
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
        var user = TestDataFactory.CreateValidStaff(placeid: 1, role: EmployeeRole.manager);
        var table = TestDataFactory.CreateValidTable(placeid: 1, label: "1", salt: "some-code");
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByPlaceLabelAsync(1, "1").Returns(table);

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
        var user = TestDataFactory.CreateValidStaff();
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
        var user = TestDataFactory.CreateValidStaff(placeid: 1, role: EmployeeRole.manager);
        var table = TestDataFactory.CreateValidTable(id: 1, placeid: 1, label: "1", salt: "qr-xyz");
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByPlaceLabelAsync(1, "1").Returns(table);

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
        await _tableRepo.Received(1).GetByPlaceLabelAsync(1, "1");
    }

    [Test]
    public async Task GetByLabelAsync_ShouldFail_WhenTableMissing()
    {
        // Arrange
        var user = TestDataFactory.CreateValidStaff(placeid: 1, role: EmployeeRole.manager);
        _userContext.GetCurrentUserAsync().Returns(user);
        _tableRepo.GetByPlaceLabelAsync(1, "Missing table").Returns((Tables?)null);

        // Act
        var result = await _service.GetByLabelAsync("Missing table");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }
}
