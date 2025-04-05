using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Bartender.Domain.Services;

public class JwtService(IConfiguration config) : IJwtService
{
    //TODO: move to .env?
    private readonly string _key = config["Jwt:Key"]!;
    private readonly string _issuer = config["Jwt:Issuer"]!;
    private readonly string _audience = config["Jwt:Audience"]!;


    public string GenerateGuestToken(int tableId, Guid sessionId, DateTime expiresAt)
    {
        var claims = new[]
        {
            new Claim("sub", "guest"),
            new Claim("table_id", tableId.ToString()),
            new Claim("session_id", sessionId.ToString())
        };

        return BuildToken(claims, expiresAt);
    }

    public string GenerateStaffToken(Staff staff)
    {
        var claims = new[]
        {
            new Claim("sub", staff.Id.ToString()),
            new Claim(ClaimTypes.Role, staff.Role.ToString()),
            new Claim("place_id", staff.PlaceId.ToString())
        };

        return BuildToken(claims, DateTime.UtcNow.AddHours(9));
    }

    private string BuildToken(IEnumerable<Claim> claims, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

