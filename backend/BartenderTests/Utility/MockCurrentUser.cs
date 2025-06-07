using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;

namespace BartenderTests.Utility;

public class MockCurrentUser : ICurrentUserContext
{
    private Staff _user;
    private bool _isGuest;
    private string? _guestToken;

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

    public int? UserId => _isGuest ? null : _user.Id;

    public bool IsGuest => _isGuest;

    public Task<Staff?> GetCurrentUserAsync() => Task.FromResult(_isGuest ? null : _user);

    public string? GetRawToken() => _guestToken;

    public void Override(Staff user)
    {
        _isGuest = false;
        _user = user;
    }

    public void OverrideGuest(string token)
    {
        _isGuest = true;
        _guestToken = token;
    }
}
