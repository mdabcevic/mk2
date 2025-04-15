using Bartender.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Table;

public class TableDto
{
    public string Label { get; set; } = string.Empty; 
    public int Seats { get; set; }

    [Required]
    public int Width { get; set; }
    [Required]
    public int Height { get; set; }
    [Required]
    public decimal X { get; set; }
    [Required]
    public decimal Y { get; set; }

    public TableStatus Status { get; set; } = TableStatus.empty;
    public string Token { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}