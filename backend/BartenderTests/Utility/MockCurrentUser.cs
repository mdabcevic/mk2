using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;

namespace BartenderTests.Utility;

public class MockCurrentUser : ICurrentUserContext
{
    private Staff _user;

    public MockCurrentUser()
    {
        _user = new Staff
        {
            Id = 999,
            Username = "testuser",
            PlaceId = 1,
            Role = EmployeeRole.owner,
            FullName = "Integration Tester",
            OIB = "00000000000",
            Password = "hashed"
        };
    }

    public int? UserId => _user.Id;

    public bool IsGuest => false;

    public Task<Staff?> GetCurrentUserAsync() => Task.FromResult<Staff?>(_user);

    public string? GetRawToken() => "fake-token";

    public void Override(Staff user)
    {
        _user = user;
    }
}
