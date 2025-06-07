using Bartender.Data.Enums;
using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IAnalyticsRepository
{
    Task<List<ProductPerOrder>> GetOrderedProductsByBusinessId(int businessId, int? placeId = null, int? month = null, int? year = null);
    Task<List<Order>> GetOrdersByBusinessId(int businessId, int? placeId = null, int? month = null, int? year = null);
    Task<List<Order>> GetOrdersByTime(DateTime dateTime, TimeFilter timeFilter, int businessId, int? placeId = null);
    Task<List<(Order order, WeatherData? weather)>> GetOrdersWithWeather(int businessId, int? placeId = null, int? month = null, int? year = null);
}
