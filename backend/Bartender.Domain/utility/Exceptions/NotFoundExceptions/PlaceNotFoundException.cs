namespace Bartender.Domain.Utility.Exceptions.NotFoundExceptions;

public class PlaceNotFoundException(int placeId) : NotFoundException($"Place with ID {placeId} was not found.")
{
    public int PlaceId { get; } = placeId;

    public override string GetLogMessage()
    {
        return $"Place with ID {PlaceId} was not found.";
    }
}
