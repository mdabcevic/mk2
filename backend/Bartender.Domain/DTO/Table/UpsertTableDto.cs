using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Table;

public class UpsertTableDto
{
    public string Label { get; set; } = string.Empty;
    public int Seats { get; set; } = 2;

    [Required]
    public int Width { get; set; }
    [Required]
    public int Height { get; set; }
    [Required]
    public decimal X { get; set; }
    [Required]
    public decimal Y { get; set; }
}
