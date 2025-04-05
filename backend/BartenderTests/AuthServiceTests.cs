using Bartender.Data.Models;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Bartender.Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;
using Bartender.Data.Enums;

namespace BartenderTests;

[TestFixture]
public class AuthServiceTests
{
    private IRepository<Staff> _repo;
    private ILogger<AuthService> _logger;
    private IJwtService _jwtService;
    private AuthService _service;

    [SetUp]
    public void SetUp()
    {
        _repo = Substitute.For<IRepository<Staff>>();
        _logger = Substitute.For<ILogger<AuthService>>();
        _jwtService = Substitute.For<IJwtService>();
        _service = new AuthService(_repo, _logger, _jwtService);
    }

    private static Staff CreateValidStaff(int id = 1) => new()
    {
        Id = id,
        PlaceId = 1,
        OIB = "12345678901",
        Username = "testuser",
        Password = BCrypt.Net.BCrypt.HashPassword("SecurePass123!"),
        FullName = "Test User",
        Role = EmployeeRole.regular
    };

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var staff = CreateValidStaff(1);

        _repo.GetByKeyAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(staff);
        _jwtService.GenerateStaffToken(staff).Returns("test-token");

        // Act
        var result = await _service.LoginAsync(new LoginStaffDto
        {
            Username = "testuser",
            Password = "SecurePass123!"
        });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.EqualTo("test-token"));
        });
    }

    [Test]
    public async Task LoginAsync_WrongPassword_ReturnsValidationError()
    {
        // Arrange
        var staff = CreateValidStaff(1);
        _repo.GetByKeyAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(staff);

        // Act
        var result = await _service.LoginAsync(new LoginStaffDto
        {
            Username = "testuser",
            Password = "wrongpass"
        });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Validation));
        });
    }

    [Test]
    public async Task LoginAsync_StaffNotFound_ReturnsValidationError()
    {
        // Arrange
        _repo.GetByKeyAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns((Staff?)null);

        // Act
        var result = await _service.LoginAsync(new LoginStaffDto
        {
            Username = "nonexistent",
            Password = "irrelevant"
        });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Validation));
        });
    }
}
