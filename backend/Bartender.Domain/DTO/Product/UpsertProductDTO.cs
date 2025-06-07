using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Product;

public class UpsertProductDto
{
    [Required]
    public required string Name { get; set; }
    public string? Volume {  get; set; }
    [Required]
    public int CategoryId { get; set; }
    public int? BusinessId { get; set; }
}
