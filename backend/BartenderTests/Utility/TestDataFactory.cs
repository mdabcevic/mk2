using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Business;
using Bartender.Domain.DTO.MenuItem;
using Bartender.Domain.DTO.Order;
using Bartender.Domain.DTO.Place;
using Bartender.Domain.DTO.Product;
using Bartender.Domain.DTO.Staff;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BartenderTests.Utility;

public static class TestDataFactory
{
    public static Staff CreateValidStaff(int id = 1, int placeid = 1, int businessid = 1, string username = "testuser",
        string password = "testpassword", EmployeeRole role = EmployeeRole.regular)
    {
        var business = CreateValidBusiness(businessid);
        var place = CreateValidPlace(placeid);
        place.Business = business;

        return new Staff
        {
            Id = id,
            PlaceId = placeid,
            OIB = "12345678901",
            Username = username,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = "Test User",
            Role = role,
            Place = place
        };
    }

    public static UpsertStaffDto CreateValidUpsertStaffDto(int id = 1, int placeid = 1, string username = "testusername",
        string password = "testpassword", EmployeeRole role = EmployeeRole.regular) 
        => new()
    {
        Id = id,
        PlaceId = placeid,
        OIB = "12345678901",
        Username = username,
        Password = password,
        FirstName = "Test",
        LastName = "User",
        Role = role
    };

    public static LoginStaffDto CreateLoginDto(string username = "testuser", string password = "SecurePass123!") => new()
    {
        Username = username,
        Password = password
    };

    public static Business CreateValidBusiness(int id = 1, string oib = "12345678901", string name = "test name", SubscriptionTier sub = SubscriptionTier.basic) => new()
    {
        Id = id,
        OIB = oib,
        Name = name,
        Headquarters = "HQ",
        SubscriptionTier = sub,
        Places = []
    };

    public static City CreateValidCity(int id = 5) => new()
    {
        Id = id,
        Name = "Zagreb"
    };


    public static MenuItem CreateValidMenuItem(int id = 1, int placeId = 1, int productId = 1, string name = "Espresso", bool isAvailable = true) => new()
    {
        Id = id,
        PlaceId = placeId,
        ProductId = productId,
        Price = 1.50m,
        IsAvailable = isAvailable,
        Description = "Strong and black",
        Product = CreateValidProduct(productId, name: name)
    };

    public static Place CreateValidPlace(int id = 1, int businessid = 1, int cityid = 1) => new()
    {
        Id = id,
        BusinessId = businessid,
        CityId = cityid,
        Address = "Test Address",
        OpensAt = new TimeOnly(8, 0),
        ClosesAt = new TimeOnly(16, 0),
        Business = CreateValidBusiness(),
        City = CreateValidCity(),
        MenuItems = [CreateValidMenuItem(id)]
    };

    public static InsertPlaceDto CreateValidInsertPlaceDto(int businessid = 1, int cityid = 1) => new()
    {
        BusinessId = businessid,
        CityId = cityid,
        Address = "Test Address",
        OpensAt = "08:00",
        ClosesAt = "16:00"
    };

    public static UpdatePlaceDto CreateValidUpdatePlaceDto() => new()
    {
        Address = "Updated Address",
        OpensAt = "09:00",
        ClosesAt = "17:00"
    };

    public static Table CreateValidTable(int id = 1, int placeid = 1, string label = "1", 
        string salt = "somesalt", TableStatus status = TableStatus.occupied, bool disabled = false)
    {
        return new Table
        {
            Id = id,
            Label = label,
            PlaceId = placeid,
            QrSalt = salt,
            Status = status,
            IsDisabled = disabled
        };
    }

    public static GuestSession CreateValidGuestSession(Table table, string token = "guest-token", int expiresInMinutes = 5)
    {
        return new GuestSession
        {
            Id = Guid.NewGuid(),
            TableId = table.Id,
            Table = table,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes)
        };
    }

    public static BusinessDto CreateBusinessDtoFromEntity(Business business) => new()
    {
        OIB = business.OIB,
        Name = business.Name,
        Headquarters = business.Headquarters,
        SubscriptionTier = business.SubscriptionTier,
        Places = []
    };

    public static ProductCategory CreateValidProductCategory(
        int id = 2,
        string name = "Coffee",
        List<Product>? products = null
        ) => new()
        {
            Id = id,
            Name = name,
            Products = products
        };

    public static Product CreateValidProduct(int id = 1, int? businessId = 1, int categoryId = 2, string name = "Espresso", string volume = "ŠAL")
    {
        var category = CreateValidProductCategory(categoryId);
        return new Product
        {
            Id = id,
            Name = name,
            Volume = volume,
            BusinessId = businessId,
            CategoryId = categoryId,
            Category = category
        };
    }

    public static ProductDto CreateValidProductDto(int id = 1, string name = "Espresso", string volume = "ŠAL", int categoryId = 2, string categoryName = "Coffee")
    {
        return new ProductDto
        {
            Id = id,
            Name = name,
            Volume = volume,
            Category = new ProductCategoryDto { Id = categoryId, Name = categoryName }
        };
    }

    public static ProductBaseDto CreateProductBaseDto(int id, string name = "Unnamed", string volume = "S")
    {
        return new ProductBaseDto
        {
            Id = id,
            Name = name,
            Volume = volume
        };
    }

    public static UpsertProductDto CreateValidUpsertProductDto(
    string name = "New Product",
    string volume = "1L",
    int categoryId = 1,
    int? businessId = null
) => new()
{
    Name = name,
    Volume = volume,
    CategoryId = categoryId,
    BusinessId = businessId
};

    public static Product CreateMappedProductFromDto(UpsertProductDto dto)
    {
        return new Product
        {
            Name = dto.Name,
            Volume = dto.Volume,
            CategoryId = dto.CategoryId,
            BusinessId = dto.BusinessId
        };
    }

    public static MenuItemBaseDto CreateMenuItemBaseDto(
    int id = 1,
    int productId = 1,
    string name = "Unnamed",
    string volume = "S",
    string? category = "Coffee",
    decimal price = 1.50m,
    string description = "Sample desc",
    bool isAvailable = true)
    {
        return new MenuItemBaseDto
        {
            Id = id,
            Product = new ProductBaseDto
            {
                Id = productId,
                Name = name,
                Volume = volume,
                Category = category
            },
            Price = price,
            Description = description,
            IsAvailable = isAvailable
        };
    }
    public static List<MenuItem> CreateSampleMenuItems(int placeId = 1) =>
       [
            CreateValidMenuItem(1, placeId, 1, "Latte"),
            CreateValidMenuItem(2, placeId, 2, "Americano")
       ];

    public static List<MenuItemBaseDto> CreateSampleMenuItemBaseDtos() =>
        [
            CreateMenuItemBaseDto(2, 2, "Americano"),
            CreateMenuItemBaseDto(1, 1, "Latte")
        ];

    public static MenuItemDto CreateMenuItemDto(
     int id,
     ProductBaseDto product,
     PlaceDto place,
     decimal price = 1.50m,
     string? description = "Sample",
     bool isAvailable = true)
    {
        return new MenuItemDto
        {
            Id = id,
            Product = product,
            Place = place,
            Price = price,
            Description = description,
            IsAvailable = isAvailable
        };
    }
    public static ProductBaseDto CreateProductBaseDtoFromProduct(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Volume = product.Volume ?? "",
        Category = product.Category?.Name ?? "Uncategorized"
    };

    public static PlaceDto CreatePlaceDtoFromPlace(Place place) => new()
    {
        BusinessName = place.Business?.Name ?? "Unknown",
        Address = place.Address,
        CityName = place.City?.Name ?? "Unknown",
        WorkHours = $"{place.OpensAt:hh\\:mm} - {place.ClosesAt:hh\\:mm}"
    };

    public static UpsertMenuItemDto CreateValidUpsertMenuItemDto(
    int placeId = 1,
    int productId = 1,
    decimal price = 2.00m,
    string description = "Strong",
    bool isAvailable = true)
    {
        return new UpsertMenuItemDto
        {
            PlaceId = placeId,
            ProductId = productId,
            Price = price,
            Description = description,
            IsAvailable = isAvailable
        };
    }

    public static UpsertMenuItemDto CreateUpsertMenuItemDto(
    int placeId = 1,
    int productId = 10,
    decimal price = 2.50m,
    string description = "Updated desc",
    bool isAvailable = true)
    {
        return new UpsertMenuItemDto
        {
            PlaceId = placeId,
            ProductId = productId,
            Price = price,
            Description = description,
            IsAvailable = isAvailable
        };
    }

    public static Order CreateValidOrder(
    int id = 1,
    int tableId = 1,
    OrderStatus status = OrderStatus.created,
    PaymentType paymentType = PaymentType.cash,
    decimal totalPrice = 10.00m,
    string? note = null)
    {
        return new Order
        {
            Id = id,
            TableId = tableId,
            Status = status,
            PaymentType = paymentType,
            TotalPrice = totalPrice,
            Note = note,
            Products = [] // optional, can add if needed for mapping
        };
    }

    public static OrderDto CreateValidOrderDto(
        int id = 1,
        OrderStatus status = OrderStatus.created,
        PaymentType paymentType = PaymentType.cash,
        decimal totalPrice = 10.00m,
        string? note = null,
        string tableLabel = "1")
    {
        return new OrderDto
        {
            Id = id,
            Status = status,
            PaymentType = paymentType,
            TotalPrice = totalPrice,
            Note = note,
            Table = tableLabel,
            CreatedAt = DateTime.UtcNow.ToShortDateString(),
            Items = [] // Add item DTOs if you're testing item-related logic
        };
    }

    public static UpsertOrderDto CreateValidUpsertOrderDto(
    int tableId = 1,
    int menuItemId = 1,
    int count = 1,
    decimal totalPrice = 1.5m,
    PaymentType paymentType = PaymentType.cash,
    string? note = null)
    {
        return new UpsertOrderDto
        {
            TableId = tableId,
            Items =
        [
            new() {
                MenuItemId = menuItemId,
                Count = count
            }
        ],
            TotalPrice = totalPrice,
            PaymentType = paymentType,
            Note = note
        };
    }

    public static UpdateOrderStatusDto CreateUpdateStatusDto(
    OrderStatus newStatus = OrderStatus.payment_requested,
    PaymentType? paymentType = null)
    {
        return new UpdateOrderStatusDto
        {
            Status = newStatus,
            PaymentType = paymentType
        };
    }

}

