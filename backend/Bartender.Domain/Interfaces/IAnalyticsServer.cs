
using Bartender.Data.Enums;
using Bartender.Domain.DTO.Analytics;

namespace Bartender.Domain.Interfaces;

public interface IAnalyticsServer
{
    Task<List<ProductsByDayOfWeekDto>> GetPopularProductsByDayOfWeek(int? placeId = null, int? month = null, int? year = null);
    Task<List<TrafficByDayOfWeekDto>> GetTrafficByDayOfWeek(int? placeId = null, int? month = null, int? year = null);
    Task<List<HourlyTrafficDto>> GetHourlyTraffic(int? placeId = null, int? month = null, int? year = null);
    Task<List<TableTrafficDto>> GetTableTraffic(int placeId, int? month = null, int? year = null);
    Task<List<PlaceTrafficDto>> GetAllPlacesTraffic(int? month = null, int? year = null);
    //Task<decimal> GetTotalEarnings(DateTime? dateTime, TimeFilter? timeFilter = TimeFilter.Day, int? placeId = null);
    Task<KeyValuesDto> GetAllInfo(int? placeId = null, int? month = null, int? year = null);

}
