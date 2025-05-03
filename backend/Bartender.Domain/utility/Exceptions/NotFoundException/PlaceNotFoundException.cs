
namespace Bartender.Domain.utility.Exceptions.NotFoundException;

public class PlaceNotFoundException : NotFoundException
{
    public int PlaceId { get; }

    public PlaceNotFoundException(int placeId, object? data = null)
        : base($"Place was not found.", data)
    {
        PlaceId = placeId;
    }

    public override string GetLogMessage()
    {
        return $"Place with ID {PlaceId} was not found.";
    }
}
