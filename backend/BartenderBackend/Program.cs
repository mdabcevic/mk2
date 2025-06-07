using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Domain;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Mappings;
using Bartender.Domain.Repositories;
using Bartender.Domain.Services;
using Bartender.Domain.Services.Data;
using Bartender.Domain.utility.ExceptionHandlers;
using Bartender.Domain.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization; // <-- ensure this namespace is included

var builder = WebApplication.CreateBuilder(args);


var allowedOrigins = "AllowedOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(allowedOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:8080",
                "https://bartender.jollywater-cb9f5de7.germanywestcentral.azurecontainerapps.io", "https://definite-squid-29206.upstash.io")
                  .AllowAnyHeader()
                  .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                  .AllowCredentials()
                  .WithExposedHeaders("Authorization");
        });
});

// Initialize Serilog 
builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, services, config) => config
    .ReadFrom.Configuration(context.Configuration)
);

var jwtSettingsSection = builder.Configuration.GetSection("Jwt");
var jwtSettings = jwtSettingsSection.Get<JwtSettings>() ?? throw new InvalidOperationException("Missing Jwt configuration.");

builder.Services.Configure<JwtSettings>(jwtSettingsSection); // for IOptions<JwtSettings> injection
builder.Services.AddSingleton(jwtSettings); // optional: direct injection without IOptions

// Configure Redis connection
var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisSettings>() ?? throw new InvalidOperationException("Missing Redis configuration.");
builder.Services.AddSingleton(redisSettings);
var configurationOptions = new ConfigurationOptions
{
    EndPoints = { $"{redisSettings.Host}:{redisSettings.Port}" },
    Password = redisSettings.Password,
    Ssl = redisSettings.Ssl,
    AbortOnConnectFail = redisSettings.AbortOnConnectFail,
    KeepAlive = 180, // ping every 3 minutes
    ReconnectRetryPolicy = new ExponentialRetry(5000),
    ConnectTimeout = 10000, // 10 seconds
    SyncTimeout = 10000,    // 10 seconds
};

var multiplexer = await ConnectionMultiplexer.ConnectAsync(configurationOptions);

builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
builder.Services.AddSingleton(redisSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key)),
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {

                var accessToken = context.Request.Query["access_token"];

                if (string.IsNullOrEmpty(accessToken))
                {
                    var authorizationHeader = context.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
                    {
                        accessToken = authorizationHeader.Substring("Bearer ".Length);
                    }
                }

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/place"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        o =>
        {
            o.MapEnum<EmployeeRole>("employeerole");
            o.MapEnum<SubscriptionTier>("subscriptiontier");
            o.MapEnum<TableStatus>("tablestatus");
            o.MapEnum<OrderStatus>("orderstatus");
            o.MapEnum<PaymentType>("paymenttype");
            o.MapEnum<ImageType>("picturetype");
            o.MapEnum<WeatherType>("weathertype");
        }));

builder.Services.AddExceptionHandler<AuthorizationExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<ConflictExceptionHandler>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

//builder.Services.AddProblemDetails();


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<ITableInteractionService, TableInteractionService>();
builder.Services.AddScoped<ITableManagementService, TableManagementService>();
builder.Services.AddScoped<IGuestSessionService, GuestSessionService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPlaceImageService, PlaceImageService>();

builder.Services.AddHttpContextAccessor(); // required!

builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITableSessionService, GuestSessionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IAnalyticsServer, AnalyticsService>();
builder.Services.AddHttpClient<IGeoCodingService, GeoCodingService>();
builder.Services.AddHttpClient<IWeatherApiService, WeatherApiService>();

builder.Services.AddSignalR();

builder.Services.AddAutoMapper(
    typeof(BusinessProfile).Assembly,
    typeof(ProductProfile).Assembly,
    typeof(MenuItemProfile).Assembly,
    typeof(PlaceProfile).Assembly,
    typeof(OrderProfile).Assembly
);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        //options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter()); //TODO: add for better time serialization / deserialization
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();


// adding weather history into database
/*using (var scope = app.Services.CreateScope())
{
    try
    {
        var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherApiService>();
        var cityRepository = scope.ServiceProvider.GetRequiredService<IRepository<City>>();

        var city = await cityRepository.GetByKeyAsync(c => c.Id == 1);

        if (city == null)
        {
            Console.WriteLine("City not found");
        }
        else
        {
            await weatherService.SaveWeatherHistory(city, new DateOnly(2025, 01, 01), new DateOnly(2025, 06, 07));
            Console.WriteLine("Weather history saved.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving weather history: {ex.Message}");
    }
}*/



app.UseSerilogRequestLogging(); // Log all HTTP requests automatically

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Swagger"));
}

app.UseRouting();
app.UseCors(allowedOrigins);
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["Access-Control-Allow-Origin"] = "https://bartender.jollywater-cb9f5de7.germanywestcentral.azurecontainerapps.io";
        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        return Task.CompletedTask;
    });
    await next();
});

app.UseHttpsRedirection();
app.UseExceptionHandler(_ => { });
app.UseAuthentication(); // <--- MUST come before UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.MapHub<PlaceHub>("/hubs/place").RequireCors(allowedOrigins);

app.MapGet("/health", () => Results.Ok("Application is healthy."))
   .WithName("HealthCheck")
   .WithTags("System");

await app.RunAsync();
