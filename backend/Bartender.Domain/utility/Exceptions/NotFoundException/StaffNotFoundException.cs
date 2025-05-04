namespace Bartender.Domain.utility.Exceptions;

public class StaffNotFoundException : NotFoundException
{
    public int StaffId { get; }

    public StaffNotFoundException(int staffId)
        : base($"Staff was not found.")
    {
        StaffId = staffId;
    }

    public override string GetLogMessage()
    {
        return $"Staff with ID {StaffId} was not found.";
    }
}
