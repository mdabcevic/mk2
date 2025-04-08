namespace Bartender.Domain.DTO.Table;

public class TableScanDto
{
    public string GuestToken { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Passphrase { get; set; } // only filled for first guest

}
