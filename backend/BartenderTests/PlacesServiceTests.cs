using AutoMapper;
using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Mappings;
using Bartender.Domain.Services;
using Bartender.Domain.Services.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Table = Bartender.Data.Models.Table;

namespace BartenderTests;


[TestFixture]
public class PlacesServiceTests
{
    private IRepository<Place> _repository;
    private IRepository<PlaceImage> _pictureRepository;
    private ITableRepository _tableRepository;
    private ILogger<PlaceService> _logger;
    private ICurrentUserContext _userContext;
    INotificationService _notificationService;
    private IValidationService _validationService;
    private IMapper _mapper;
    private PlaceService _service;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Place>>();
        _pictureRepository = Substitute.For<IRepository<PlaceImage>>();
        _tableRepository = Substitute.For<ITableRepository>();
        _logger = Substitute.For<ILogger<PlaceService>>();
        _userContext = Substitute.For<ICurrentUserContext>();
        _notificationService = Substitute.For<INotificationService>();
        _validationService = Substitute.For<IValidationService>();


        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PlaceProfile>();
        });
        _mapper = config.CreateMapper();

        _service = new PlaceService(_repository, _pictureRepository, _tableRepository, _logger, _userContext, _notificationService, _validationService, _mapper);
    }


    [Test]
    public async Task AddAsync_Should_Add_Place_When_Authorized()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidInsertPlaceDto();
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(10, role:EmployeeRole.admin));

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).AddAsync(Arg.Any<Place>());
    }

    [Test]
    public async Task AddAsync_Should_Return_Unauthorized_When_CrossBusiness()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidInsertPlaceDto(businessid: 1);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(id: 99, businessid: 99, placeid: 99, role: EmployeeRole.admin));

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _repository.DidNotReceive().AddAsync(Arg.Any<Place>());
    }

    [Test]
    public async Task DeleteAsync_Should_Delete_When_Found_And_Authorized()
    {
        // Arrange
        var place = TestDataFactory.CreateValidPlace();
        _repository.GetByIdAsync(place.Id).Returns(place);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(10));

        // Act
        var result = await _service.DeleteAsync(place.Id);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).DeleteAsync(place);
    }

    [Test]
    public async Task DeleteAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        _repository.GetByIdAsync(1).Returns((Place?)null);

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
    public async Task DeleteAsync_Should_Return_Unauthorized_When_CrossBusiness()
    {
        // Arrange
        var place = TestDataFactory.CreateValidPlace();
        place.BusinessId = 999;
        _repository.GetByIdAsync(1).Returns(place);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(10));

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
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
        var result = await _service.UpdateAsync(place.Id, dto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).UpdateAsync(place);
    }

    [Test]
    public async Task UpdateAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        _repository.GetByIdAsync(1).Returns((Place?)null);

        // Act
        var result = await _service.UpdateAsync(1, TestDataFactory.CreateValidUpdatePlaceDto());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task UpdateAsync_Should_Return_Unauthorized_When_CrossBusiness()
    {
        // Arrange
        var place = TestDataFactory.CreateValidPlace();
        place.BusinessId = 999;
        _repository.GetByIdAsync(1).Returns(place);
        _userContext.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(10));

        // Act
        var result = await _service.UpdateAsync(1, TestDataFactory.CreateValidUpdatePlaceDto());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Place>());
    }

    [Test]
    public async Task NotifyStaffAsync_Should_SendNotification_WhenTableFound()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(label: "A1", salt: "salt123");
        _tableRepository.GetBySaltAsync("salt123").Returns(table);

        // Act
        var result = await _service.NotifyStaffAsync("salt123");

        // Assert
        Assert.That(result.Success, Is.True);
        await _notificationService.Received().AddNotificationAsync(table, Arg.Any<TableNotification>());
    }

    [Test]
    public async Task NotifyStaffAsync_Should_ReturnNotFound_WhenTableMissing()
    {
        // Arrange
        _tableRepository.GetBySaltAsync("salt123").Returns((Table?)null);

        // Act
        var result = await _service.NotifyStaffAsync("salt123");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
        await _notificationService.DidNotReceive().AddNotificationAsync(Arg.Any<Table>(), Arg.Any<TableNotification>());
    }
}

