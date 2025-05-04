using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Table;

namespace Bartender.Domain.Interfaces;

public interface ITableManagementService
{
    Task<List<TableDto>> GetAllAsync();
    Task<TableDto> GetByLabelAsync(string label);
    Task DeleteAsync(string label);
    Task BulkUpsertAsync(List<UpsertTableDto> dtoList);
    Task SwitchDisabledAsync(string label, bool flag);
    Task<string> RegenerateSaltAsync(string label);
    Task<List<BaseTableDto>> GetByPlaceId(int placeId);
}
