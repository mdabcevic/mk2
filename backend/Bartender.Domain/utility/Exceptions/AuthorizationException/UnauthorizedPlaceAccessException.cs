
namespace Bartender.Domain.utility.Exceptions;

public class UnauthorizedPlaceAccessException : AuthorizationException
{
    public int? PlaceId;

    public UnauthorizedPlaceAccessException(int? placeId = null)
        : base($"Access to place denied")
    {
        PlaceId = placeId;
    }

    public override string GetLogMessage()
    {
        return PlaceId.HasValue ?
            $"Unathorized access to place with ID {PlaceId}" :
            $"Cross-entity request detected";
    }
}
