
using Bartender.Domain.DTO.Table;

namespace Bartender.Domain.DTO.Analytics;

public class TableTrafficDto
{
    public required BaseTableDto Table{ get; set; }
    public required int Count  { get; set; }
    public required decimal AverageRevenue { get; set; }
}
