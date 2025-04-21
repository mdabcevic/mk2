using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Staff;

namespace BartenderTests;

public static class TestDataFactory
{
    public static Staff CreateValidStaff(int id = 1, int placeid = 1, string username = "testuser",
        string password = "testpassword", EmployeeRole role = EmployeeRole.regular)
    {
        var business = CreateValidBusiness();
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

    public static Places CreateValidPlace(int id = 1, int businessId = 1) => new()
    {
        Id = id,
        BusinessId = businessId,
        CityId = 1,
        Address = "Some St 5",
        OpensAt = new TimeOnly(7, 0),
        ClosesAt = new TimeOnly(17, 0),
        Business = CreateValidBusiness(businessId),
        City = new Cities { Id = 1, Name = "Zagreb" },
        MenuItems = []
    };
}

