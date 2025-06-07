using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Microsoft.Extensions.Logging;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.DTO.Analytics;
using AutoMapper;
using Bartender.Domain.DTO.Table;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using System.Collections.Concurrent;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;

namespace Bartender.Domain.Services;

public class AnalyticsService(
    IAnalyticsRepository repository,
    ILogger<OrderService> logger,
    ICurrentUserContext currentUser,
    IValidationService validationService,
    IMapper mapper
    ) : IAnalyticsServer
{
    private ConcurrentDictionary<(int businessId, int? placeId, int? month, int? year), List<Order>> _ordersCache = new ConcurrentDictionary<(int, int?, int?, int?), List<Order>>();


    private async Task<List<Order>> GetOrdersCached(int businessId, int? placeId, int? month, int? year)
    {
        var key = (businessId, placeId, month, year);

        if (_ordersCache.TryGetValue(key, out var cachedOrders))
        {
            return cachedOrders;
        }

        var orders = await repository.GetOrdersByBusinessId(businessId, placeId, month, year);
        _ordersCache.TryAdd(key, orders);

        return orders;
    }

    public async Task<List<ProductsByDayOfWeekDto>> GetPopularProductsByDayOfWeek(int? placeId = null, int? month = null, int? year = null)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId(placeId);

        var products = await repository.GetOrderedProductsByBusinessId(businessId, placeId, month, year);
        var topProducts = products
            .GroupBy(o => new { weekDay = o.Order.CreatedAt.DayOfWeek, product = o.MenuItem.Product })
            .Select(g => new PopularProductsDto
            {
                DayOfWeek = g.Key.weekDay.ToString(),
                ProductId = g.Key.product.Id,
                Product = g.Key.product.Name,
                Count = g.Sum(o => o.Count),
                Earnings = g.Sum(o => (o.Count * o.Price)/ (1 - o.Discount / 100m))
            })
            .GroupBy(p => p.DayOfWeek)
            .Select(g => new ProductsByDayOfWeekDto
            {
                DayOfWeek = g.Key,
                PopularProducts = g
                    .OrderByDescending(p => p.Count)
                    .Take(5)
                    .ToList()
            })
            .OrderBy(g => GetDayOfWeekOrder(g.DayOfWeek)) 
            .ToList();

        var overallTopProducts = products
            .GroupBy(o => o.MenuItem.Product)
            .Select(g => new PopularProductsDto
            {
                DayOfWeek = "All",
                ProductId = g.Key.Id,
                Product = g.Key.Name,
                Count = g.Sum(o => o.Count),
                Earnings = g.Sum(o => (o.Count * o.Price) / (1 - o.Discount / 100m))
            })
            .OrderByDescending(p => p.Count)
            .Take(5)
            .ToList();

        topProducts.Add(new ProductsByDayOfWeekDto
        {
            DayOfWeek = "All",
            PopularProducts = overallTopProducts
        });


        return topProducts;
    }

    public async Task<List<TrafficByDayOfWeekDto>> GetTrafficByDayOfWeek(int? placeId = null, int? month = null, int? year = null)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId(placeId);

        var orders = await GetOrdersCached(businessId, placeId, month, year);
        var traffic = orders
            .GroupBy(o => o.CreatedAt.DayOfWeek)
            .Select(g => new TrafficByDayOfWeekDto
            {
                DayOfWeek = g.Key.ToString(),
                Count = g.Count(),
                Earnings = g.Sum(o => o.TotalPrice)
            })
            .OrderBy(g => GetDayOfWeekOrder(g.DayOfWeek))
            .ToList();

        return traffic;
    }

    public async Task<List<HourlyTrafficDto>> GetHourlyTraffic(int? placeId = null, int? month = null, int? year = null)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId(placeId);

        var orders = await GetOrdersCached(businessId, placeId, month, year);
        var traffic = orders
            .GroupBy(o => o.CreatedAt.DayOfWeek)
            .Select(dayGroup => new HourlyTrafficDto
            {
                DayOfWeek = dayGroup.Key.ToString(),
                HourCounts = dayGroup
                    .GroupBy(o => o.CreatedAt.Hour)
                    .Select(hourGroup => new HourCountDto
                    {
                        Hour = hourGroup.Key,
                        Count = hourGroup.Count(),
                        AverageEarnings = hourGroup.Average(o => o.TotalPrice)
                    })
                .OrderBy(h => h.Hour)
                .ToList()
            })
            .OrderBy(g => GetDayOfWeekOrder(g.DayOfWeek))
            .ToList();

        return traffic;
    }

    public async Task<List<TableTrafficDto>> GetTableTraffic(int placeId, int? month = null, int? year = null)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId(placeId);

        var orders = await GetOrdersCached(businessId, placeId, month, year);
        var traffic = orders
            .GroupBy(o => o.Table)
            .Select(g => new TableTrafficDto
            {
                Table = mapper.Map<BaseTableDto>(g.Key),
                Count = g.Count(),
                AverageEarnings = Math.Round(g.Average(o => o.TotalPrice), 2)
            })
            .ToList();

        return traffic;
    }

    public async Task<List<PlaceTrafficDto>> GetAllPlacesTraffic(int? month = null, int? year = null)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId();

        var orders = await GetOrdersCached(businessId, null, month, year);
        var traffic = orders
            .GroupBy(o => o.Table.Place)
            .Select(g => new PlaceTrafficDto
            {
                PlaceId = g.Key.Id,
                Address = g.Key?.Address ?? "Unknown",
                CityName = g.Key?.City?.Name ?? "Unknown",
                Lat = g.Key?.Latitude,
                Long = g.Key?.Longitude,
                Count = g.Count(),
                Earnings = g.Sum(o => o.TotalPrice)
            })
            .ToList();

        return traffic;
    }


    public async Task<KeyValuesDto> GetAllInfo(int? placeId = null, int? month = null, int? year = null)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId(placeId);

        //var orders = await repository.GetOrdersByTime(dateTime.Value, timeFilter.Value, businessId, placeId);
        var orders = await GetOrdersCached(businessId, placeId, month, year);
        var orders2 = await GetOrdersCached(businessId, placeId, null, null);

        var keyValues = new KeyValuesDto
        {
            Earnings = orders.Sum(o => o.TotalPrice),
            AverageOrder = Math.Round(orders.Average(o => o.TotalPrice), 2),
            TotalOrders = orders.Count(),
            FirstEverOrderDate = orders2.OrderBy(o => o.CreatedAt).First().CreatedAt.ToString("dd/MM/yyyy"),
            FirstOrderDate = orders.OrderBy(o => o.CreatedAt).First().CreatedAt,
            LastOrderDate = orders.OrderByDescending(o => o.CreatedAt).First().CreatedAt
        };

        return keyValues;
    }

    private async Task<int> CheckAuthorizationAndReturnBusinessId(int? placeId = null)
    {
        var user = await currentUser.GetCurrentUserAsync();
        int? businessId = null;
        if (user != null && user.Place != null)
            businessId = user.Place.BusinessId;

        if (businessId == null)
            throw new BusinessNotFoundException();

        if (placeId != null)
        {
            await validationService.EnsurePlaceExistsAsync(placeId.Value);
            /*if (!await validationService.VerifyUserPlaceAccess(placeId.Value))
                throw new UnauthorizedPlaceAccessException(placeId);*/
            if (!await validationService.VerifyUserBusinessAccess(businessId.Value))
                throw new UnauthorizedBusinessAccessException(businessId.Value);
        }

        return businessId.Value;
    }

    public async Task<List<OrdersByWeatherDto>> GetOrderWeatherAnalytics(int placeId, int? month = null, int? year = null)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId(placeId);

        var orders = await repository.GetOrdersWithWeather(businessId, placeId, month, year);
        var groupedByHour = orders
            .Where(o => o.weather != null && o.weather.WeatherType != WeatherType.unknown)
            .GroupBy(o => new
            {
                WeekGroup = (o.weather!.DateTime.DayOfWeek == DayOfWeek.Saturday || o.weather!.DateTime.DayOfWeek == DayOfWeek.Sunday) ? "Weekend" : "Weekday",
                Hour = o.weather.DateTime.Hour,
                Date = o.weather.DateTime.Date,
                WeatherType = o.weather.WeatherType
            })
            .Select(g => new
            {
                g.Key.WeekGroup,
                g.Key.WeatherType,
                g.Key.Hour,
                g.Key.Date,
                NumOfOrders = g.Count()
            })
            .ToList();

        var grouped = groupedByHour
            .GroupBy(x => new { x.WeekGroup, x.WeatherType })
            .Select(g => new
            {
                g.Key.WeekGroup,
                g.Key.WeatherType,
                TotalOrders = g.Sum(x => x.NumOfOrders),
                TotalDistinctHours = g.Select(x => new { x.Date, x.Hour }).Distinct().Count()
            })
            .Select(x => new OrdersByWeatherDto
            {
                WeekGroup = x.WeekGroup,
                WeatherType = x.WeatherType,
                AverageOrdersPerHour = x.TotalDistinctHours > 0 ? (double)x.TotalOrders / x.TotalDistinctHours : 0
            })
            .ToList();


        return grouped;
    }

    public async Task<AllAnalyticsDataDto> GetAllAnalyticsData(int? placeId, int? month, int? year)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId(placeId);

        var orders = await GetOrdersCached(businessId, placeId, month, year);

        var popularProducts = await GetPopularProductsByDayOfWeek(placeId, month, year);
        var dailyTraffic = await GetTrafficByDayOfWeek(placeId, month, year);
        var hourlyTraffic = await GetHourlyTraffic(placeId, month, year);
        var placeTraffic = await GetAllPlacesTraffic(month, year);
        var keyValues = await GetAllInfo(placeId, month, year);

        return new AllAnalyticsDataDto
        {
            PopularProducts = popularProducts,
            DailyTraffic = dailyTraffic,
            HourlyTraffic = hourlyTraffic,
            PlaceTraffic = placeTraffic,
            KeyValues = keyValues
        };
    }


    private int GetDayOfWeekOrder(string day)
    {
        return day switch
        {
            "Monday" => 1,
            "Tuesday" => 2,
            "Wednesday" => 3,
            "Thursday" => 4,
            "Friday" => 5,
            "Saturday" => 6,
            "Sunday" => 7,
            _ => 8
        };
    }
}
