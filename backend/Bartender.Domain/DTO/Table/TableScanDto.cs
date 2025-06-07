namespace Bartender.Domain.DTO.Table;

public class TableScanDto
{
    public string? GuestToken { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Message {  get; set; } = string.Empty;
    public bool IsSessionEstablished { get; set; } = false;
}
