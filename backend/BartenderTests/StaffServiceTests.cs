using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
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

    [Test]
    public async Task AddAsync_Should_Add_Staff_When_Username_Is_Unique()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertStaffDto();
        _repository.ExistsAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(false);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff());

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
        var dto = TestDataFactory.CreateValidUpsertStaffDto();
        _repository.ExistsAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(true);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff());

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
            Assert.That(result.Error, Does.Contain("already exists"));
        });
        await _repository.DidNotReceive().AddAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task AddAsync_Should_Return_Unauthorized_When_CrossBusiness()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertStaffDto();
        var user = TestDataFactory.CreateValidStaff();
        user.PlaceId = 999;
        _userContext.GetCurrentUserAsync().Returns(user);

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _repository.DidNotReceive().AddAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task DeleteAsync_Should_Remove_Staff_When_Found()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff();
        _repository.GetByIdAsync(1).Returns(staff);
        _userContext.GetCurrentUserAsync().Returns(staff); 

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
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task DeleteAsync_Should_Return_Unauthorized_When_PlaceId_Does_Not_Match_CurrentUser()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff();
        staff.PlaceId = 99; 
        _repository.GetByIdAsync(1).Returns(staff);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(1)); 

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task GetAllAsync_Should_Return_StaffDto_List()
    {
        // Arrange
        var staffList = new List<Staff> { TestDataFactory.CreateValidStaff(1), TestDataFactory.CreateValidStaff(2) };
        _repository.GetAllAsync().Returns(staffList);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(1)); 

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
        await _repository.Received(1).GetAllAsync();
    }

    [Test]
    public async Task GetByIdAsync_Should_Return_StaffDto_When_Found()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(5);
        _repository.GetByIdAsync(5, false).Returns(staff);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(5));

        // Act
        var result = await _service.GetByIdAsync(5);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        await _repository.Received(1).GetByIdAsync(5, false);

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
    public async Task GetByIdAsync_Should_Return_Unauthorized_When_PlaceId_Does_Not_Match_CurrentUser()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(5, placeid: 99);
        _repository.GetByIdAsync(5, false).Returns(staff);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(1, placeid: 1)); //TODO: simulate another user properly...

        // Act
        var result = await _service.GetByIdAsync(5);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
    }

    [Test]
    public async Task UpdateAsync_Should_Update_When_Found()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertStaffDto(id: 99);
        var staff = TestDataFactory.CreateValidStaff(id: 99);
        _repository.GetByIdAsync(99).Returns(staff);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(99));

        // Act
        var result = await _service.UpdateAsync(99, dto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).UpdateAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task UpdateAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertStaffDto(50);
        _repository.GetByIdAsync(50).Returns((Staff?)null);

        // Act
        var result = await _service.UpdateAsync(50, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task UpdateAsync_Should_Return_Unauthorized_When_PlaceId_Does_Not_Match_CurrentUser()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertStaffDto(50);
        dto.PlaceId = 99; 
        var staff = TestDataFactory.CreateValidStaff(50);
        _repository.GetByIdAsync(50).Returns(staff);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(1));

        // Act
        var result = await _service.UpdateAsync(50, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Staff>());
    }
}
