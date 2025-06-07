using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;

namespace BartenderTests.UnitTests;

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
        _repository.ExistsAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(false);  // Simulate unique username
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff());

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _service.AddAsync(dto));  // Ensure no exception is thrown
        await _repository.Received(1).AddAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task AddAsync_Should_Throw_ConflictException_When_Username_Exists()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertStaffDto();
        _repository.ExistsAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(true);  // Simulate existing username
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff());

        // Act & Assert
        var ex = Assert.ThrowsAsync<ConflictException>(async () => await _service.AddAsync(dto));
        Assert.That(ex.Message, Does.Contain("already exists"));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task AddAsync_Should_Throw_UnauthorizedBusinessAccessException_When_CrossBusiness()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertStaffDto();
        var user = TestDataFactory.CreateValidStaff();
        user.PlaceId = 999;  // Set user's place to a different business
        _userContext.GetCurrentUserAsync().Returns(user);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedBusinessAccessException>(async () => await _service.AddAsync(dto));
        Assert.That(ex.Message, Does.Contain("Access"));
        Assert.That(ex.Message, Does.Contain("denied"));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task DeleteAsync_Should_Delete_Staff_When_Found_And_Authorized()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff();
        _repository.GetByIdAsync(1).Returns(staff);  // Simulate staff exists
        _userContext.GetCurrentUserAsync().Returns(staff);  // Simulate the current user is the same as the staff to be deleted

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _service.DeleteAsync(1));  // Ensure no exception is thrown
        await _repository.Received(1).DeleteAsync(staff);  // Ensure the DeleteAsync method was called on the repository
    }

    [Test]
    public async Task DeleteAsync_Should_Throw_StaffNotFoundException_When_Staff_Missing()
    {
        // Arrange
        _repository.GetByIdAsync(1).Returns((Staff?)null);  // Simulate staff is not found

        // Act & Assert
        var ex = Assert.ThrowsAsync<StaffNotFoundException>(async () => await _service.DeleteAsync(1));
        Assert.That(ex.Message, Does.Contain("not found"));
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task DeleteAsync_Should_Throw_UnauthorizedPlaceAccessException_When_PlaceId_Does_Not_Match_CurrentUser()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff();
        staff.PlaceId = 99;  // Set staff's PlaceId to a different value
        _repository.GetByIdAsync(1).Returns(staff);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: 1));  // Set current user's PlaceId to a different value

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(async () => await _service.DeleteAsync(1));
        Assert.That(ex.Message, Does.Contain("Access"));
        Assert.That(ex.Message, Does.Contain("denied"));
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
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<List<StaffDto>>());
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result, Has.All.InstanceOf<StaffDto>());
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
        Assert.That(result, Is.Not.Null);  // Ensure the result is not null
        Assert.That(result, Is.InstanceOf<StaffDto>());  // Ensure the result is of type StaffDto
        await _repository.Received(1).GetByIdAsync(5, false);  // Ensure the repository method is called once
    }

    [Test]
    public void GetByIdAsync_Should_Throw_StaffNotFoundException_When_Staff_Missing()
    {
        // Arrange
        _repository.GetByIdAsync(10, Arg.Any<bool>()).Returns((Staff?)null);  // Simulate missing staff

        // Act & Assert
        var ex = Assert.ThrowsAsync<StaffNotFoundException>(async () => await _service.GetByIdAsync(10));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public void GetByIdAsync_Should_Throw_UnauthorizedPlaceAccessException_When_PlaceId_Does_Not_Match_CurrentUser()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(5, placeid: 99);  // Staff belongs to a different place
        _repository.GetByIdAsync(5, false).Returns(staff);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(1, placeid: 1));  // Current user belongs to a different place

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(async () => await _service.GetByIdAsync(5));
        Assert.That(ex.Message, Does.Contain("Access"));
        Assert.That(ex.Message, Does.Contain("denied"));
    }

    [Test]
    public async Task UpdateAsync_Should_Update_When_Found()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertStaffDto(id: 99);
        var staff = TestDataFactory.CreateValidStaff(id: 99);
        _repository.GetByIdAsync(99).Returns(staff);  // Simulate staff is found
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(id: 99));  // Simulate the current user is the same as the staff to be updated

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _service.UpdateAsync(99, dto));  // Ensure no exception is thrown
        await _repository.Received(1).UpdateAsync(Arg.Any<Staff>());  // Ensure UpdateAsync is called once on the repository
    }

    [Test]
    public async Task UpdateAsync_Should_Throw_StaffNotFoundException_When_Staff_Missing()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertStaffDto(50);  // DTO for a staff with ID 50
        _repository.GetByIdAsync(50).Returns((Staff?)null);  // Simulate that the staff with ID 50 doesn't exist

        // Act & Assert
        var ex = Assert.ThrowsAsync<StaffNotFoundException>(async () => await _service.UpdateAsync(50, dto));
        Assert.That(ex.Message, Does.Contain("not found"));
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Staff>());
    }

    [Test]
    public async Task UpdateAsync_Should_Throw_UnauthorizedPlaceAccessException_When_PlaceId_Does_Not_Match_CurrentUser()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertStaffDto(50);  // Create DTO with PlaceId 99
        dto.PlaceId = 99;  // Set PlaceId to a different value than the current user's PlaceId
        var staff = TestDataFactory.CreateValidStaff(50);  // Create staff with PlaceId 99
        _repository.GetByIdAsync(50).Returns(staff);  // Simulate that staff exists in a different place
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(1));  // Simulate current user in place 1

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(async () => await _service.UpdateAsync(50, dto));
        Assert.That(ex.Message, Does.Contain("Access"));
        Assert.That(ex.Message, Does.Contain("denied"));
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Staff>());
    }
}
