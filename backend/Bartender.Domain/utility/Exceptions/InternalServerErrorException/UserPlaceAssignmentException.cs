
namespace Bartender.Domain.utility.Exceptions;

public class UserPlaceAssignmentException : UnknownErrorException
{
    public int? UserId { get; }
    public UserPlaceAssignmentException(int? userId, object? data = null)
        : base($"Error fetching user's place.", data)
    {
        UserId = userId;
    }

    public override string GetLogMessage()
    {
        return UserId.HasValue
            ? $"User with id {UserId} isn't assigned to any place."
            : "User isn't assigned to any place.";
    }
}
