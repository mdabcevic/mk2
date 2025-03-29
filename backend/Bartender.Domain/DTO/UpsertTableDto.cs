namespace Bartender.Domain.DTO;

public class UpsertTableDto
{
    public int PlaceId { get; set; } //not needed - will pick up manually from UserID?

    public int Seats { get; set; } = 2;

    // Optional: let manager set initial state, or omit entirely and default it in service
    public string Status { get; set; } = "empty";

    // Optional: allow manager to regenerate salt manually during update
    public string? Salt { get; set; }

    public bool IsDisabled { get; set; } = false;
}
