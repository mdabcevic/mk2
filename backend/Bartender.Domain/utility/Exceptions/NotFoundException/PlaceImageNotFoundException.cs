
namespace Bartender.Domain.utility.Exceptions.NotFoundException;

public class PlaceImageNotFoundException : NotFoundException
{
    public int ImageId { get; }

    public PlaceImageNotFoundException(int imageId, object? data)
        : base($"Image was not found.", data)
    {
        ImageId = imageId;
    }

    public override string GetLogMessage()
    {
        return $"Place image with ID {ImageId} was not found.";
    }
}
