using Bartender.Domain.DTO.Table;

namespace Bartender.Domain.Interfaces;

public interface ITableManagementService
{
    Task<ServiceResult<List<TableDto>>> GetAllAsync();
    Task<ServiceResult<TableDto>> GetByLabelAsync(string label);
    Task<ServiceResult> AddAsync(UpsertTableDto dto);
    Task<ServiceResult> DeleteAsync(string label);
    Task<ServiceResult> UpdateAsync(string label, UpsertTableDto dto);
    Task<ServiceResult> BulkUpsertAsync(List<UpsertTableDto> dtoList);
    Task<ServiceResult> SwitchDisabledAsync(string label, bool flag);
    Task<ServiceResult> RegenerateSaltAsync(string label);
    Task<ServiceResult<List<TableDto>>> GetByPlaceId(int placeId);
}
