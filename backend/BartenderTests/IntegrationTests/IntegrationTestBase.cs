using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Net.Http;
using Bartender.Data;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Testcontainers.PostgreSql;
using Bartender.Data.Enums;
using Npgsql;



namespace BartenderTests.IntegrationTests;

[TestFixture]
public class IntegrationTestBase
{
    protected HttpClient TestClient;
    private PostgreSqlContainer _pgContainer;
    protected WebApplicationFactory<Program> Factory;

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

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.Migrate();
                });
            });

        TestClient = Factory.CreateClient();
        // ✅ Then: seed data from init.sql
        var initScript = await File.ReadAllTextAsync("initseed.sql");

        using var conn = new NpgsqlConnection(_pgContainer.GetConnectionString());
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand(initScript, conn);
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("✅ Test database seeded successfully.");
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        TestClient?.Dispose();
        await Factory.DisposeAsync();
        await _pgContainer.DisposeAsync();
    }
}
