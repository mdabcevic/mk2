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
            policy.WithOrigins("http://localhost:5173/", "http://localhost:8080/", "http://localhost:5173", "http://localhost:8080",
                "https://bartender.jollywater-cb9f5de7.germanywestcentral.azurecontainerapps.io")
                  .AllowAnyHeader()
                  .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                  .AllowCredentials();
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
app.UseSerilogRequestLogging(); // Log all HTTP requests automatically

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Swagger"));
}

app.UseExceptionHandler( _ => { });
app.UseHttpsRedirection();
app.UseCors(allowedOrigins);
app.UseAuthentication(); // <--- MUST come before UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.MapHub<PlaceHub>("/hubs/place");

app.MapGet("/health", () => Results.Ok("Application is healthy."))
   .WithName("HealthCheck")
   .WithTags("System");

await app.RunAsync();
