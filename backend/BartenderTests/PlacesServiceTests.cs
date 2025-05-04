using AutoMapper;
using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Mappings;
using Bartender.Domain.Services.Data;
using Bartender.Domain.Utility.Exceptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Table = Bartender.Data.Models.Table;

namespace BartenderTests;


[TestFixture]
public class PlacesServiceTests
{
    private IRepository<Place> _repository;
    private ITableRepository _tableRepository;
    private ILogger<PlaceService> _logger;
    private ICurrentUserContext _userContext;
    INotificationService _notificationService;
    private IMapper _mapper;
    private PlaceService _service;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Place>>();
        _tableRepository = Substitute.For<ITableRepository>();
        _logger = Substitute.For<ILogger<PlaceService>>();
        _userContext = Substitute.For<ICurrentUserContext>();
        _notificationService = Substitute.For<INotificationService>();


        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PlaceProfile>();
        });
        _mapper = config.CreateMapper();

        _service = new PlaceService(_repository, _tableRepository, _logger, _userContext, _notificationService, _mapper);
    }


    [Test]
    public async Task AddAsync_Should_Add_Place_When_Authorized()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidInsertPlaceDto();
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(10, role: EmployeeRole.admin));

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _service.AddAsync(dto));
        await _repository.Received(1).AddAsync(Arg.Any<Place>());
    }


    [Test]
    public async Task AddAsync_Should_Throw_UnauthorizedPlaceAccessException_When_CrossBusiness()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidInsertPlaceDto(businessid: 1);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(id: 99, businessid: 99, placeid: 99, role: EmployeeRole.admin));

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(async () => await _service.AddAsync(dto));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Place>());
    }

    [Test]
    public async Task DeleteAsync_Should_Delete_When_Found_And_Authorized()
    {
        // Arrange
        var place = TestDataFactory.CreateValidPlace();
        _repository.GetByIdAsync(place.Id).Returns(place);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(10));

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _service.DeleteAsync(place.Id));
        await _repository.Received(1).DeleteAsync(place);
    }

    [Test]
    public async Task DeleteAsync_Should_Throw_NotFoundException_When_Place_Not_Found()
    {
        // Arrange
        _repository.GetByIdAsync(1).Returns((Place?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _service.DeleteAsync(1));
        Assert.That(ex.Message, Is.EqualTo("Place with ID 1 not found.")); // Or whatever message your NotFoundException throws
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Place>());
    }

    [Test]
    public async Task DeleteAsync_Should_Throw_UnauthorizedPlaceAccessException_When_CrossBusiness()
    {
        // Arrange
        var place = TestDataFactory.CreateValidPlace();
        place.BusinessId = 999;  // Set business ID different from the current user's business
        _repository.GetByIdAsync(1).Returns(place);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(10, businessid: 10));  // Set current user with a different business ID

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(async () => await _service.DeleteAsync(1));
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Place>());
    }

    [Test]
    public async Task UpdateAsync_Should_Update_When_Found_And_Authorized()
    {
        // Arrange
        var place = TestDataFactory.CreateValidPlace();
        _repository.GetByIdAsync(place.Id).Returns(place);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(10));

        // Act
        var dto = TestDataFactory.CreateValidUpdatePlaceDto();

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _service.UpdateAsync(place.Id, dto));
        await _repository.Received(1).UpdateAsync(place);
    }

    [Test]
    public async Task UpdateAsync_Should_Throw_NotFoundException_When_Place_Not_Found()
    {
        // Arrange
        _repository.GetByIdAsync(1).Returns((Place?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _service.UpdateAsync(1, TestDataFactory.CreateValidUpdatePlaceDto()));
        Assert.That(ex.Message, Is.EqualTo("Place with ID 1 not found."));  // Assert that the exception message is correct
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Place>());
    }

    [Test]
    public async Task UpdateAsync_Should_Throw_UnauthorizedPlaceAccessException_When_CrossBusiness()
    {
        // Arrange
        var place = TestDataFactory.CreateValidPlace();
        place.BusinessId = 999;  // Set business ID different from the current user's business
        _repository.GetByIdAsync(1).Returns(place);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(10, businessid: 10));  // Set current user with a different business ID

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(async () => await _service.UpdateAsync(1, TestDataFactory.CreateValidUpdatePlaceDto()));
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Place>());
    }

    [Test]
    public async Task NotifyStaffAsync_Should_SendNotification_When_Table_Found()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(label: "A1", salt: "salt123");
        _tableRepository.GetBySaltAsync("salt123").Returns(table);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _service.NotifyStaffAsync("salt123"));
        await _notificationService.Received(1).AddNotificationAsync(table, Arg.Any<TableNotification>());
    }

    [Test]
    public async Task NotifyStaffAsync_Should_Throw_NotFoundException_When_Table_Missing()
    {
        // Arrange
        _tableRepository.GetBySaltAsync("salt123").Returns((Table?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _service.NotifyStaffAsync("salt123"));
        Assert.That(ex.Message, Is.EqualTo("Table with salt 'salt123' not found."));
        await _notificationService.DidNotReceive().AddNotificationAsync(Arg.Any<Table>(), Arg.Any<TableNotification>());
    }
}

