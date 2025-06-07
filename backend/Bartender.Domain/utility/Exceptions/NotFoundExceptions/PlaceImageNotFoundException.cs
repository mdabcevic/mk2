namespace Bartender.Domain.Utility.Exceptions.NotFoundExceptions;

public class PlaceImageNotFoundException(int imageId) : NotFoundException($"Image was not found.")
{
    public int ImageId { get; } = imageId;

    public override string GetLogMessage()
    {
        return $"Place image with ID {ImageId} was not found.";
    }
}
