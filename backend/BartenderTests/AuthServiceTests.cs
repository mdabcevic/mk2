using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;
using Bartender.Domain.DTO;

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

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(1, username: "testuser", password: "testpassword");
        _repo.GetByKeyAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(staff);
        _jwtService.GenerateStaffToken(staff).Returns("test-token");

        // Act
        var result = await _service.LoginAsync(TestDataFactory.CreateLoginDto("testuser", "testpassword"));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.EqualTo("test-token"));
        });
        _jwtService.Received(1).GenerateStaffToken(staff);
    }

    [TestCase("", TestName = "WrongPassword_Empty")]
    [TestCase("wrongpassword", TestName = "WrongPassword")]
    public async Task LoginAsync_WrongPassword_ReturnsValidationError(string password)
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(1, username: "testuser", password: "testpassword");
        _repo.GetByKeyAsync(Arg.Any<Expression<Func<Staff, bool>>>()).Returns(staff);
        var loginDto = TestDataFactory.CreateLoginDto("testuser", password);

        // Act
        var result = await _service.LoginAsync(loginDto);

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
        var result = await _service.LoginAsync(TestDataFactory.CreateLoginDto("nonexistent", "irrelevant"));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Validation));
        });
    }
}
