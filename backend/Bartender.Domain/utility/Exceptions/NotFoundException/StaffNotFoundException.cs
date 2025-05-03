namespace Bartender.Domain.utility.Exceptions.NotFoundException;

public class StaffNotFound : NotFoundException
{
    public int StaffId { get; }

    public StaffNotFound(int staffId)
        : base($"Staff was not found.")
    {
        StaffId = staffId;
    }

    public override string GetLogMessage()
    {
        return $"Staff with ID {StaffId} was not found.";
    }
}
