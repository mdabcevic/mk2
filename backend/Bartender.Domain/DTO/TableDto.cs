using Bartender.Data.Enums;

namespace Bartender.Domain.DTO;

public class TableDto
{
    //public int Id { get; set; } 
    public string Label { get; set; } = string.Empty; 
    public int Seats { get; set; }
    public TableStatus Status { get; set; } = TableStatus.empty;
    public string Token { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}