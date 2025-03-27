using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;

namespace BartenderTests;

[TestFixture]
class StaffServiceTests
{
    private IRepository<Staff> _repository;
    private ILogger<StaffService> _logger;
    private IMapper _mapper;
    private StaffService _service;
    private ICurrentUserContext _userContext;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Staff>>();
        _logger = Substitute.For<ILogger<StaffService>>();
        _userContext = Substitute.For<ICurrentUserContext>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Staff, StaffDto>();
            cfg.CreateMap<UpsertStaffDto, Staff>()
                .ForMember(dest => dest.FullName, opt =>
                    opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        });
        _mapper = config.CreateMapper();

        _service = new StaffService(_repository, _logger, _userContext, _mapper);
    }

    private static Staff CreateValidStaff(int id = 1) => new()
    {
        Id = id,
        PlaceId = 10,
        OIB = "12345678901",
        Username = $"staff{id}",
        Password = "SecurePass123!",
        FullName = "Test User",
        Role = EmployeeRole.regular
    };

    private static UpsertStaffDto CreateValidUpsertStaffDto(int id = 1) => new()
    {
        Id = id,
        PlaceId = 10,
        OIB = "12345678901",
        Username = $"staff{id}",
        Password = "SecurePass123!",
        FirstName = "Test",
        LastName = "User",
        Role = EmployeeRole.regular
    };

    [Test]
    public async Task AddAsync_Should_Add_Staff_When_Username_Is_Unique()
    {
        // Arrange
        var dto = CreateValidUpsertStaffDto();
        _repository.ExistsAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(false);
        _userContext.GetCurrentUserAsync().Returns(CreateValidStaff());

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).AddAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task AddAsync_Should_Return_Conflict_When_Username_Exists()
    {
        // Arrange
        var dto = CreateValidUpsertStaffDto();
        _repository.ExistsAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(true);
        _userContext.GetCurrentUserAsync().Returns(CreateValidStaff());

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
            Assert.That(result.Error, Does.Contain("already exists"));
        });
    }

    [Test]
    public async Task DeleteAsync_Should_Remove_Staff_When_Found()
    {
        // Arrange
        var staff = CreateValidStaff();
        _repository.GetByIdAsync(1).Returns(staff);
        _userContext.GetCurrentUserAsync().Returns(staff); // PlaceId matches

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).DeleteAsync(staff);
    }

    [Test]
    public async Task DeleteAsync_Should_Return_NotFound_When_Staff_Missing()
    {
        // Arrange
        _repository.GetByIdAsync(1).Returns((Staff?)null);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task GetAllAsync_Should_Return_StaffDto_List()
    {
        // Arrange
        var staffList = new List<Staff> { CreateValidStaff(1), CreateValidStaff(2) };
        _repository.GetAllAsync().Returns(staffList);
        _userContext.GetCurrentUserAsync().Returns(CreateValidStaff(1)); // PlaceId = 10

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.TypeOf<List<StaffDto>>());
        });
        Assert.That(result.Data, Has.Count.EqualTo(2));
        Assert.That(result.Data, Has.All.InstanceOf<StaffDto>());
    }

    [Test]
    public async Task GetByIdAsync_Should_Return_StaffDto_When_Found()
    {
        // Arrange
        var staff = CreateValidStaff(5);
        _repository.GetByIdAsync(5, false).Returns(staff);
        _userContext.GetCurrentUserAsync().Returns(CreateValidStaff(5));

        // Act
        var result = await _service.GetByIdAsync(5);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data?.Username, Is.EqualTo("staff5"));
        });
        
    }

    [Test]
    public async Task GetByIdAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        _repository.GetByIdAsync(10, Arg.Any<bool>()).Returns((Staff?)null);

        // Act
        var result = await _service.GetByIdAsync(10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task UpdateAsync_Should_Update_When_Found()
    {
        // Arrange
        var dto = CreateValidUpsertStaffDto(99);
        var staff = CreateValidStaff(99);
        _repository.GetByIdAsync(99).Returns(staff);
        _userContext.GetCurrentUserAsync().Returns(CreateValidStaff(99));

        // Act
        var result = await _service.UpdateAsync(99, dto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received().UpdateAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task UpdateAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        var dto = CreateValidUpsertStaffDto(50);
        _repository.GetByIdAsync(50).Returns((Staff?)null);

        // Act
        var result = await _service.UpdateAsync(50, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }
}
