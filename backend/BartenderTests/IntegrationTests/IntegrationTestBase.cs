using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Bartender.Data;
using Testcontainers.PostgreSql;
using Npgsql;
using Bartender.Domain.Interfaces;
using BartenderTests.Utility;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bartender.Domain.Utility;
using System.Net.Http.Headers;
using Bartender.Domain.DTO.Staff;
using System.Net.Http.Json;

namespace BartenderTests.IntegrationTests;

[TestFixture]
public class IntegrationTestBase
{
    protected HttpClient TestClient;
    private PostgreSqlContainer _pgContainer;
    protected WebApplicationFactory<Program> Factory;
    protected virtual bool UseMockCurrentUser => true;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        _pgContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _pgContainer.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseNpgsql(_pgContainer.GetConnectionString());
                    });

                    if (UseMockCurrentUser)
                    {
                        var existing = services.SingleOrDefault(s => s.ServiceType == typeof(ICurrentUserContext));
                        if (existing != null)
                            services.Remove(existing);

                        services.AddScoped<MockCurrentUser>();
                        services.AddScoped<ICurrentUserContext>(sp => sp.GetRequiredService<MockCurrentUser>());
                    }

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.Migrate();
                });
            });

        TestClient = Factory.CreateClient();
        // seed data from init.sql
        var initScript = await File.ReadAllTextAsync("initseed.sql");

        using var conn = new NpgsqlConnection(_pgContainer.GetConnectionString());
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand(initScript, conn);
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("✅ Test database seeded successfully.");

        var jwt = Factory.Services.GetRequiredService<JwtSettings>();
        var token = GenerateTestToken(jwt.Key);
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        TestClient?.Dispose();
        await Factory.DisposeAsync();
        await _pgContainer.DisposeAsync();
    }

    private static string GenerateTestToken(string key, int placeId = 1, string role = "manager")
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "99"),
        new Claim(ClaimTypes.Role, role),
        new Claim("PlaceId", placeId.ToString())
    };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected void SetAuthHeader(string role = "manager", int placeId = 1)
    {
        var jwt = Factory.Services.GetRequiredService<JwtSettings>();
        var token = GenerateTestToken(jwt.Key, placeId, role);
        TestClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task<string> LoginAndGetTokenAsync(string username, string password)
    {
        var loginDto = new LoginStaffDto { Username = username, Password = password };
        var response = await TestClient.PostAsJsonAsync("/api/auth", loginDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
