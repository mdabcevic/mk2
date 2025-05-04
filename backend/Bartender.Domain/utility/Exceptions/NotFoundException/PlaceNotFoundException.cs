
namespace Bartender.Domain.Utility.Exceptions;

public class PlaceNotFoundException : NotFoundException
{
    public int PlaceId { get; }

    public PlaceNotFoundException(int placeId)
        : base($"Place with ID {placeId} was not found.")
    {
        PlaceId = placeId;
    }

    public override string GetLogMessage()
    {
        return $"Place with ID {PlaceId} was not found.";
    }
}
