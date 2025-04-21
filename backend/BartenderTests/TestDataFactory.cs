using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Staff;
using System.Diagnostics.Eventing.Reader;

namespace BartenderTests;

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

    public static Businesses CreateValidBusiness(int id = 1, string oib = "12345678901", string name = "test name", SubscriptionTier sub = SubscriptionTier.basic) => new()
    {
        Id = id,
        OIB = oib,
        Name = name,
        Headquarters = "HQ",
        SubscriptionTier = sub,
        Places = []
    };

    public static Cities CreateValidCity(int id = 5) => new()
    {
        Id = id,
        Name = "Zagreb"
    };

    public static Products CreateValidProduct(int id = 1) => new()
    {
        Id = id,
        Name = "Espresso"
    };

    private static MenuItems CreateValidMenuItem(int placeId = 1, int productId = 1) => new()
    {
        PlaceId = placeId,
        ProductId = productId,
        Price = 1.50m,
        IsAvailable = true,
        Description = "Strong and black",
        Product = CreateValidProduct(productId)
    };

    public static Places CreateValidPlace(int id = 1, int businessid = 1, int cityid = 1) => new()
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

    public static Tables CreateValidTable(int id = 1, int placeid = 1, string label = "1", 
        string salt = "somesalt", TableStatus status = TableStatus.occupied, bool disabled = false)
    {
        return new Tables
        {
            Id = id,
            Label = label,
            PlaceId = placeid,
            QrSalt = salt,
            Status = status,
            IsDisabled = disabled
        };
    }

    public static GuestSession CreateValidGuestSession(Tables table, string token = "guest-token", int expiresInMinutes = 5)
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

    public static BusinessDto CreateBusinessDtoFromEntity(Businesses business) => new()
    {
        OIB = business.OIB,
        Name = business.Name,
        Headquarters = business.Headquarters,
        SubscriptionTier = business.SubscriptionTier,
        Places = []
    };
}

