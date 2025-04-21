using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;


namespace BartenderTests;

[TestFixture]
public class GuestSessionServiceTests
{
    private IRepository<GuestSession> _sessionRepo;
    IRepository<GuestSessionGroup> _groupSessionRepo;
    private IJwtService _jwtService;
    private ILogger<GuestSessionService> _logger;
    private GuestSessionService _service;

    [SetUp]
    public void SetUp()
    {
        _sessionRepo = Substitute.For<IRepository<GuestSession>>();
        _groupSessionRepo = Substitute.For<IRepository<GuestSessionGroup>>();
        _jwtService = Substitute.For<IJwtService>();
        _logger = Substitute.For<ILogger<GuestSessionService>>();
        _service = new GuestSessionService(_sessionRepo, _groupSessionRepo, _jwtService, _logger);
    }

    [Test]
    public async Task HasActiveSessionAsync_ShouldReturnTrue_WhenActiveExists()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable();
        var token = "guest token";
        var session = TestDataFactory.CreateValidGuestSession(table, token: token);
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        // Act
        var result = await _service.HasActiveSessionAsync(1, token);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasActiveSessionAsync_ShouldReturnFalse_WhenNoActiveSession()
    {
        // Arrange
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns((GuestSession?)null);

        // Act
        var result = await _service.HasActiveSessionAsync(1, "invalid-token");

        // Assert
        Assert.That(result, Is.False);
    }


    //[Test]
    //public async Task CanResumeExpiredSessionAsync_ShouldReturnTrue_IfLatestExpiredMatches()
    //{
    //    var latestExpired = new GuestSession
    //    {
    //        TableId = 1,
    //        Token = "expired.token",
    //        ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
    //    };

    //    _sessionRepo.Query().Returns(new List<GuestSession> { latestExpired }.AsQueryable());

    //    var result = await _service.CanResumeExpiredSessionAsync(1, "expired.token");

    //    Assert.That(result, Is.True);
    //}

    //[Test]
    //public async Task CanResumeExpiredSessionAsync_ShouldReturnFalse_IfLatestExpiredDoesNotMatch()
    //{
    //    var latestExpired = new GuestSession
    //    {
    //        TableId = 1,
    //        Token = "expired.token",
    //        ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
    //    };

    //    _sessionRepo.Query().Returns(new List<GuestSession> { latestExpired }.AsQueryable());

    //    var result = await _service.CanResumeExpiredSessionAsync(1, "wrong.token");

    //    Assert.That(result, Is.False);
    //}

    //[Test]
    //public async Task CreateSessionAsync_ShouldCreateSessionAndReturnToken()
    //{
    //    // Arrange
    //    var passphrase = "any passphrase";
    //    _jwtService.GenerateGuestToken(1, Arg.Any<Guid>(), Arg.Any<DateTime>(), passphrase)
    //        .Returns("mock.token");

    //    // Act
    //    string token = await _service.CreateSessionAsync(1, passphrase);

    //    // Assert
    //    Assert.That(token, Is.EqualTo("mock.token"));
    //    await _sessionRepo.Received().AddAsync(Arg.Any<GuestSession>());
    //}

    [Test]
    public async Task DeleteSessionAsync_ShouldRemoveSession_WhenExists()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new GuestSession { Id = sessionId };

        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        // Act
        await _service.DeleteSessionAsync(sessionId);

        // Assert
        await _sessionRepo.Received().DeleteAsync(session);
    }

    [Test]
    public async Task DeleteSessionAsync_ShouldDoNothing_WhenSessionMissing()
    {
        // Arrange
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns((GuestSession?)null);

        // Act
        await _service.DeleteSessionAsync(Guid.NewGuid());

        // Assert
        await _sessionRepo.DidNotReceive().DeleteAsync(Arg.Any<GuestSession>());
    }

    [Test]
    public async Task GetByTokenAsync_ShouldReturnMatchingSession()
    {
        // Arrange
        var session = new GuestSession { TableId = 1, Token = "abc" };
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        // Act
        var result = await _service.GetByTokenAsync(1, "abc");

        // Assert
        Assert.That(result, Is.EqualTo(session));
    }
}
