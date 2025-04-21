using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Mappings;
using Bartender.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BartenderTests;


[TestFixture]
public class PlacesServiceTests
{
    private IRepository<Places> _repository;
    private ITableRepository _tableRepository;
    private ILogger<PlacesService> _logger;
    private ICurrentUserContext _userContext;
    INotificationService _notificationService;
    private IMapper _mapper;
    private PlacesService _service;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Places>>();
        _tableRepository = Substitute.For<ITableRepository>();
        _logger = Substitute.For<ILogger<PlacesService>>();
        _userContext = Substitute.For<ICurrentUserContext>();
        _notificationService = Substitute.For<INotificationService>();
        

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PlacesProfile>();
        });
        _mapper = config.CreateMapper();

        _service = new PlacesService(_repository, _tableRepository, _logger, _userContext, _notificationService, _mapper);
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
        await _repository.Received(1).AddAsync(Arg.Any<Places>());
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
        await _repository.DidNotReceive().AddAsync(Arg.Any<Places>());
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
        _repository.GetByIdAsync(1).Returns((Places?)null);

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
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Places>());
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
        _repository.GetByIdAsync(1).Returns((Places?)null);

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
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Places>());
    }
}

