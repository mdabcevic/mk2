
using Bartender.Data.Enums;

namespace Bartender.Domain.DTO.Table;

public class BaseTableDto
{
    public string Label { get; set; } = string.Empty;
    public int Seats { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public TableStatus Status { get; set; } = TableStatus.empty;
}
