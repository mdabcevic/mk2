namespace Bartender.Domain.Utility.Exceptions;

public class UserPlaceAssignmentException(int? userId, object? data = null) : UnknownErrorException($"Error fetching user's place.", data)
{
    public int? UserId { get; } = userId;

    public override string GetLogMessage()
    {
        return UserId.HasValue
            ? $"User with id {UserId} isn't assigned to any place."
            : "User isn't assigned to any place.";
    }
}
