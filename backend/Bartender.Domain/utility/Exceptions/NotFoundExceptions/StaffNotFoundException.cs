namespace Bartender.Domain.Utility.Exceptions.NotFoundExceptions;

public class StaffNotFoundException(int staffId) : NotFoundException($"Staff was not found.")
{
    public int StaffId { get; } = staffId;

    public override string GetLogMessage()
    {
        return $"Staff with ID {StaffId} was not found.";
    }
}
