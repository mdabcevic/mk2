using Bartender.Domain.DTO.Table;

namespace Bartender.Domain.Interfaces;

public interface ITableManagementService
{
    Task<ServiceResult<List<TableDto>>> GetAllAsync();
    Task<ServiceResult<TableDto>> GetByLabelAsync(string label);
    Task<ServiceResult> DeleteAsync(string label);
    Task<ServiceResult> BulkUpsertAsync(List<UpsertTableDto> dtoList);
    Task<ServiceResult> SwitchDisabledAsync(string label, bool flag);
    Task<ServiceResult<string>> RegenerateSaltAsync(string label);
    Task<ServiceResult<List<BaseTableDto>>> GetByPlaceId(int placeId);
}
