namespace Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;

public class UnauthorizedPlaceAccessException(int? placeId = null) : AuthorizationException($"Access to place denied")
{
    public int? PlaceId = placeId;

    public override string GetLogMessage()
    {
        return PlaceId.HasValue ?
            $"Unathorized access to place with ID {PlaceId}" :
            $"Cross-entity request detected";
    }
}
