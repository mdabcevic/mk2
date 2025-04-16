namespace Bartender.Domain.DTO.Table;

public class TableDto : BaseTableDto
{
    public string Token { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}