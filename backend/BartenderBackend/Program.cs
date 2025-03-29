using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Domain;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Mappings;
using Bartender.Domain.Repositories;
using Bartender.Domain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization; // <-- ensure this namespace is included

var builder = WebApplication.CreateBuilder(args);

// Initialize Serilog 
builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, services, config) => config
    .ReadFrom.Configuration(context.Configuration)
);

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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
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
        }));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IPlacesService, PlacesService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();

builder.Services.AddHttpContextAccessor(); // required!
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

builder.Services.AddAutoMapper(
    typeof(BusinessProfile).Assembly
);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        //options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(
    typeof(ProductProfile),
    typeof(MenuItemProfile),
    typeof(PlacesProfile)
    );

var app = builder.Build();
app.UseSerilogRequestLogging(); // Log all HTTP requests automatically

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Swagger"));
}

app.UseHttpsRedirection();
app.UseAuthentication(); // <--- MUST come before UseAuthorization
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();
