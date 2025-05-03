
namespace Bartender.Domain.utility.Exceptions.NotFoundException;

public class PlaceNotFoundException : NotFoundException
{
    public int PlaceId { get; }

    public PlaceNotFoundException(int placeId)
        : base($"Place was not found.")
    {
        PlaceId = placeId;
    }

    public override string GetLogMessage()
    {
        return $"Place with ID {PlaceId} was not found.";
    }
}
