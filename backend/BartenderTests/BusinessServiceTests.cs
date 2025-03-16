using BartenderBackend.Models;
using BartenderBackend.Repositories;
using BartenderBackend.Services;
using NSubstitute;

namespace BartenderTests;

[TestFixture]
public class BusinessServiceTests
{
    private IRepository<Business> _repository;
    private BusinessService _businessService;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Business>>();
        _businessService = new BusinessService(_repository);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsBusiness_WhenExists()
    {
        // Arrange
        var business = new Business { Id = 1, OIB = "12345678901", Name = "Test Bar" };
        _repository.GetByIdAsync(1).Returns(Task.FromResult((Business?)business));

        // Act
        var result = await _businessService.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.OIB, Is.EqualTo("12345678901"));
        });
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        _repository.GetByIdAsync(1).Returns(Task.FromResult((Business?)null));

        // Act
        var result = await _businessService.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllBusinesses()
    {
        // Arrange
        var businesses = new List<Business>
        {
            new() { Id = 1, OIB = "12345678901", Name = "Test Bar 1" },
            new() { Id = 2, OIB = "23456789012", Name = "Test Bar 2" }
        };

        _repository.GetAllAsync().Returns(Task.FromResult(businesses));

        // Act
        var result = await _businessService.GetAllAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task AddAsync_AddsBusinessSuccessfully()
    {
        // Arrange
        var business = new Business { Id = 1, OIB = "12345678901", Name = "Test Bar" };

        // Act
        await _businessService.AddAsync(business);

        // Assert
        await _repository.Received(1).AddAsync(business);
    }

    [Test]
    public void AddAsync_ThrowsException_WhenOIBInvalid()
    {
        // Arrange
        var business = new Business { Id = 1, OIB = "123", Name = "Test Bar" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _businessService.AddAsync(business));
        Assert.That(ex.Message, Is.EqualTo("OIB must be 11 characters"));
    }

    [Test]
    public async Task UpdateAsync_UpdatesBusinessSuccessfully()
    {
        // Arrange
        var business = new Business { Id = 1, OIB = "12345678901", Name = "Updated Bar" };

        // Act
        await _businessService.UpdateAsync(business);

        // Assert
        await _repository.Received(1).UpdateAsync(business);
    }

    [Test]
    public async Task DeleteAsync_DeletesExistingBusiness()
    {
        // Arrange
        var business = new Business { Id = 1, OIB = "12345678901", Name = "Test Bar" };
        _repository.GetByIdAsync(1).Returns(Task.FromResult((Business?)business));

        // Act
        await _businessService.DeleteAsync(1);

        // Assert
        await _repository.Received(1).DeleteAsync(business);
    }

    [Test]
    public async Task DeleteAsync_DoesNothing_WhenBusinessNotExists()
    {
        // Arrange
        _repository.GetByIdAsync(1).Returns(Task.FromResult((Business?)null));

        // Act
        await _businessService.DeleteAsync(1);

        // Assert
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Business>());
    }
}
