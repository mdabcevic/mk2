using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
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

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Staff>>();
        _logger = Substitute.For<ILogger<StaffService>>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Staff, StaffDto>();
            cfg.CreateMap<UpsertStaffDto, Staff>();
        });
        _mapper = config.CreateMapper();

        _service = new StaffService(_repository, _logger, _mapper);
    }

    private static Staff CreateValidStaff(int id = 1)
    {
        return new Staff
        {
            Id = id,
            PlaceId = 10,
            OIB = "12345678901",
            Username = $"staff{id}",
            Password = "SecurePass123!",
            FullName = "Test User",
            Role = EmployeeRole.regular
        };
    }

    private static StaffDto CreateValidStaffDto(int id = 1)
    {
        return new StaffDto
        {
            OIB = "12345678901",
            Username = $"staff{id}",
            FullName = "Test User",
            Role = EmployeeRole.regular
        };
    }

    private static UpsertStaffDto CreateValidUpsertStaffDto(int id = 1)
    {
        return new UpsertStaffDto
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
    }

    [Test]
    public async Task AddAsync_Should_Add_Staff_When_Username_Is_Unique()
    {
        // Arrange
        var dto = CreateValidUpsertStaffDto();

        _repository.ExistsAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(false);

        // Act
        await _service.AddAsync(dto);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<Staff>());
    }

    [Test]
    public void AddAsync_Should_Throw_When_Username_Exists()
    {
        var dto = CreateValidUpsertStaffDto();
        _repository.ExistsAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(true);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(dto));
        Assert.That(ex.Message, Does.Contain("already exists"));
    }

    [Test]
    public async Task DeleteAsync_Should_Remove_Staff_When_Found()
    {
        var staff = CreateValidStaff(1);

        _repository.GetByIdAsync(1).Returns(staff);

        await _service.DeleteAsync(1);

        await _repository.Received(1).DeleteAsync(staff);
    }

    [Test]
    public void DeleteAsync_Should_Throw_When_Not_Found()
    {
        _repository.GetByIdAsync(1, Arg.Any<bool>()).Returns((Staff?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(1));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task GetAllAsync_Should_Return_StaffDto_List()
    {

        var staffList = new List<Staff>
        {
            CreateValidStaff(1),
            CreateValidStaff(2)
        };
        _repository.GetAllAsync().Returns(staffList);

        var result = await _service.GetAllAsync();

        Assert.That(result, Has.All.InstanceOf<StaffDto>());
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Username, Is.EqualTo("staff1"));
            Assert.That(result[0].GetType().GetProperty("Password"), Is.Null);
        });
    }

    [Test]
    public async Task GetByIdAsync_Should_Return_StaffDto_When_Found()
    {
        // Arrange
        var staff = CreateValidStaff(5);

        _repository.GetByIdAsync(5, false).Returns(staff);

        // Act
        var result = await _service.GetByIdAsync(5);

        // Assert
        Assert.That(result, Is.InstanceOf<StaffDto>());
        Assert.Multiple(() =>
        {
            Assert.That(result.GetType().GetProperty("Password"), Is.Null);
            Assert.That(result?.Username, Is.EqualTo("staff5"));
        });
    }

    [Test]
    public void GetByIdAsync_Should_Throw_When_Not_Found()
    {
        _repository.GetByIdAsync(10).Returns((Staff?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(10));
        Assert.That(ex.Message, Contains.Substring("not found"));
    }

    [Test]
    public async Task UpdateAsync_Should_Update_When_Found()
    {
        var dto = CreateValidUpsertStaffDto();
        var staff = CreateValidStaff(99);

        _repository.GetByIdAsync(99).Returns(staff);

        await _service.UpdateAsync(99, dto);

        await _repository.Received().UpdateAsync(staff);
    }

    [Test]
    public void UpdateAsync_Should_Throw_When_Not_Found()
    {
        _repository.GetByIdAsync(50).Returns((Staff?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(50, CreateValidUpsertStaffDto(50)));
        Assert.That(ex.Message, Contains.Substring("not found"));
    }
}
