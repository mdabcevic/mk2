using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
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

    [Test]
    public async Task GetConflictingSessionAsync_ShouldReturnSession_WhenExists()
    {
        var session = new GuestSession { TableId = 2, Token = "xyz", IsValid = true };
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        var result = await _service.GetConflictingSessionAsync("xyz", 1);

        Assert.That(result, Is.EqualTo(session));
    }

    [Test]
    public void CreateSessionAsync_ShouldThrow_WhenPassphraseMissing()
    {
        // Arrange
        var tableId = 1;

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateSessionAsync(tableId, null!));
        Assert.That(ex!.ParamName, Is.EqualTo("passphrase"));
    }

    [Test]
    public void CreateSessionAsync_ShouldThrow_WhenPassphraseIncorrect()
    {
        // Arrange
        var tableId = 1;
        var existingGroup = new GuestSessionGroup { TableId = tableId, Passphrase = "correct" };

        _groupSessionRepo.Query().Returns(new List<GuestSessionGroup> { existingGroup }.AsQueryable());

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateSessionAsync(tableId, "wrong")
        );

        Assert.That(ex!.Message, Is.EqualTo("Incorrect passphrase for this table."));
    }

    [Test]
    public async Task RevokeSessionAsync_ShouldSetInvalid_WhenSessionExists()
    {
        var session = new GuestSession { Id = Guid.NewGuid(), IsValid = true };
        _sessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(session);

        await _service.RevokeSessionAsync(session.Id);

        Assert.That(session.IsValid, Is.False);
        await _sessionRepo.Received().UpdateAsync(session);
    }

    //[Test]
    //public async Task RevokeAllSessionsForTableAsync_ShouldRevokeAllValidSessions()
    //{
    //    var sessions = new List<GuestSession>
    //{
    //    new() { Id = Guid.NewGuid(), TableId = 1, IsValid = true, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(5) },
    //    new() { Id = Guid.NewGuid(), TableId = 1, IsValid = true, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(10) },
    //};

    //    _sessionRepo.Query().Returns(sessions.AsQueryable());

    //    await _service.RevokeAllSessionsForTableAsync(1);

    //    Assert.That(sessions.All(s => s.IsValid == false));
    //    await _sessionRepo.Received(1).UpdateRangeAsync(Arg.Is<List<GuestSession>>(list => list.Count == 2));
    //}

    //[Test]
    //public async Task EndGroupSessionAsync_ShouldRevokeSessionsAndDeleteGroup()
    //{
    //    var group = new GuestSessionGroup { Id = Guid.NewGuid(), TableId = 1 };
    //    _groupSessionRepo.Query().Returns(new List<GuestSessionGroup> { group }.AsQueryable());

    //    _sessionRepo.Query().Returns(new List<GuestSession>
    //{
    //    new() { Id = Guid.NewGuid(), TableId = 1, IsValid = true, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(1) }
    //}.AsQueryable());

    //    await _service.EndGroupSessionAsync(1);

    //    await _sessionRepo.Received(1).UpdateRangeAsync(Arg.Any<List<GuestSession>>());
    //    await _groupSessionRepo.Received(1).DeleteAsync(group);
    //}
}
