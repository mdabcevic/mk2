
namespace Bartender.Domain.utility.Exceptions;

public class UnauthorizedPlaceAccessException : AuthorizationException
{
    public int? PlaceId;
    public UnauthorizedPlaceAccessException()
        : base($"Failure fetching place with requested id.")
    {
    }
    public UnauthorizedPlaceAccessException(int placeId)
        : base($"Failure fetching place with requested id.")
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
