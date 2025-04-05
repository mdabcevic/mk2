using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.Services;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;

namespace BartenderTests;

[TestFixture]
public class JwtServiceTests
{
    private JwtService _service;

    [SetUp]
    public void SetUp()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "SuperSecureSecretKey123456789HowManyMoreCharacterDoesItNeedToWork" },
                { "Jwt:Issuer", "testissuerbackend" },
                { "Jwt:Audience", "testissuerfrontend" }
            })
            .Build();

        _service = new JwtService(config);
    }
    private static Staff CreateValidStaff(int id = 1, EmployeeRole role = EmployeeRole.regular) => new()
    {
        Id = id,
        PlaceId = 1,
        OIB = "12345678901",
        Username = "testuser",
        Password = BCrypt.Net.BCrypt.HashPassword("SecurePass123!"),
        FullName = "Test User",
        Role = role
    };

    [Test]
    public void GenerateStaffToken_ReturnsValidJwt()
    {
        // Arrange
        var staff = CreateValidStaff(12, EmployeeRole.owner);

        // Act
        var token = _service.GenerateStaffToken(staff);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Assert
        Assert.That(jwt, Is.Not.Null);

        var claims = jwt.Claims.ToList();

        Assert.Multiple(() =>
        {
            Assert.That(claims.Any(c => c.Type == "place_id" && c.Value == "1"), Is.True);
            Assert.That(claims.Any(c => c.Type == "sub" && c.Value == "12"), Is.True);
            Assert.That(claims.Any(c => c.Type.Contains("role") && c.Value == "owner"), Is.True);

            Assert.That(jwt.Issuer, Is.EqualTo("testissuerbackend"));
            Assert.That(jwt.Audiences, Does.Contain("testissuerfrontend"));
        });
    }

    [Test]
    public void GenerateGuestToken_ReturnsValidJwt()
    {
        // Arrange
        var tableId = 42;
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(30);

        // Act
        var token = _service.GenerateGuestToken(tableId, sessionId, expiresAt);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var claims = jwt.Claims.ToList();

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(claims.Any(c => c.Type == "table_id" && c.Value == tableId.ToString()), Is.True);
            Assert.That(claims.Any(c => c.Type == "session_id" && c.Value == sessionId.ToString()), Is.True);
            Assert.That(claims.Any(c => c.Type == "sub" && c.Value == "guest"), Is.True);

            Assert.That(jwt.ValidTo, Is.EqualTo(expiresAt).Within(TimeSpan.FromSeconds(5)));
        });
    }

}
