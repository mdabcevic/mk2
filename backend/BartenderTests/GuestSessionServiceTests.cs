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
    private IJwtService _jwtService;
    private ILogger<GuestSessionService> _logger;
    private GuestSessionService _service;

    [SetUp]
    public void SetUp()
    {
        _sessionRepo = Substitute.For<IRepository<GuestSession>>();
        _jwtService = Substitute.For<IJwtService>();
        _logger = Substitute.For<ILogger<GuestSessionService>>();
        _service = new GuestSessionService(_sessionRepo, _jwtService, _logger);
    }

    [Test]
    public async Task HasActiveSessionAsync_ShouldReturnTrue_WhenActiveExists()
    {
        var session = new GuestSession { TableId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(10) };
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        var result = await _service.HasActiveSessionAsync(1);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasActiveSessionAsync_ShouldReturnFalse_WhenNoActiveSession()
    {
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns((GuestSession?)null);

        var result = await _service.HasActiveSessionAsync(1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsSameTokenAsActiveAsync_ShouldReturnTrue_WhenTokenMatches()
    {
        var session = new GuestSession
        {
            TableId = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Token = "match"
        };
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        var result = await _service.IsSameTokenAsActiveAsync(1, "match");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsSameTokenAsActiveAsync_ShouldReturnFalse_WhenTokenDiffers()
    {
        var session = new GuestSession
        {
            TableId = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Token = "actual"
        };
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        var result = await _service.IsSameTokenAsActiveAsync(1, "wrong");

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

    [Test]
    public async Task CreateSessionAsync_ShouldCreateSessionAndReturnToken()
    {
        _jwtService.GenerateGuestToken(1, Arg.Any<Guid>(), Arg.Any<DateTime>())
            .Returns("mock.token");

        string token = await _service.CreateSessionAsync(1);

        Assert.That(token, Is.EqualTo("mock.token"));
        await _sessionRepo.Received().AddAsync(Arg.Is<GuestSession>(s =>
            s.TableId == 1 &&
            s.Token == "mock.token" &&
            s.ExpiresAt > DateTime.UtcNow
        ));
    }

    [Test]
    public async Task DeleteSessionAsync_ShouldRemoveSession_WhenExists()
    {
        var sessionId = Guid.NewGuid();
        var session = new GuestSession { Id = sessionId };

        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        await _service.DeleteSessionAsync(sessionId);

        await _sessionRepo.Received().DeleteAsync(session);
    }

    [Test]
    public async Task DeleteSessionAsync_ShouldDoNothing_WhenSessionMissing()
    {
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns((GuestSession?)null);

        await _service.DeleteSessionAsync(Guid.NewGuid());

        await _sessionRepo.DidNotReceive().DeleteAsync(Arg.Any<GuestSession>());
    }

    //[Test]
    //public async Task GetLatestExpiredSessionAsync_ShouldReturnLatestExpired()
    //{
    //    var expired = new GuestSession { TableId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(-1) };

    //    _sessionRepo.Query().Returns(new List<GuestSession> { expired }.AsQueryable());

    //    var result = await _service.GetLatestExpiredSessionAsync(1);

    //    Assert.That(result, Is.EqualTo(expired));
    //}

    [Test]
    public async Task GetByTokenAsync_ShouldReturnMatchingSession()
    {
        var session = new GuestSession { TableId = 1, Token = "abc" };
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        var result = await _service.GetByTokenAsync(1, "abc");

        Assert.That(result, Is.EqualTo(session));
    }
}
