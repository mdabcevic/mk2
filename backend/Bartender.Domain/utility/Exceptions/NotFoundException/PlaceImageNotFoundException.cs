
namespace Bartender.Domain.Utility.Exceptions;

public class PlaceImageNotFoundException : NotFoundException
{
    public int ImageId { get; }

    public PlaceImageNotFoundException(int imageId)
        : base($"Image was not found.")
    {
        ImageId = imageId;
    }

    public override string GetLogMessage()
    {
        return $"Place image with ID {ImageId} was not found.";
    }
}
