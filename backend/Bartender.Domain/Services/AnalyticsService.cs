using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Microsoft.Extensions.Logging;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.DTO.Analytics;
using AutoMapper;
using Bartender.Domain.DTO.Table;
using Bartender.Data.Enums;

namespace Bartender.Domain.Services;

public class AnalyticsService(
    IAnalyticsRepository repository,
    ILogger<OrderService> logger,
    ICurrentUserContext currentUser,
    IValidationService validationService,
    IMapper mapper
    ) : IAnalyticsServer
{
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

        return topProducts;
    }

    public async Task<List<TrafficByDayOfWeekDto>> GetTrafficByDayOfWeek(int? placeId = null, int? month = null, int? year = null)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId(placeId);

        var orders = await repository.GetOrdersByBusinessId(businessId, placeId, month, year);
        var traffic = orders
            .GroupBy(o => o.CreatedAt.DayOfWeek)
            .Select(g => new TrafficByDayOfWeekDto
            {
                DayOfWeek = g.Key.ToString(),
                Count = g.Count(),
                AverageEarnings = g.Average(o => o.TotalPrice)
            })
            .OrderBy(g => GetDayOfWeekOrder(g.DayOfWeek))
            .ToList();

        return traffic;
    }

    public async Task<List<HourlyTrafficDto>> GetHourlyTraffic(int? placeId = null, int? month = null, int? year = null)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId(placeId);

        var orders = await repository.GetOrdersByBusinessId(businessId, placeId, month, year);
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

        var orders = await repository.GetOrdersByBusinessId(businessId, placeId, month, year);
        var traffic = orders
            .GroupBy(o => o.Table)
            .Select(g => new TableTrafficDto
            {
                Table = mapper.Map<BaseTableDto>(g.Key),
                Count = g.Count(),
                AverageEarnings = g.Average(o => o.TotalPrice)
            })
            .ToList();

        return traffic;
    }

    public async Task<List<PlaceTrafficDto>> GetAllPlacesTraffic(int? month = null, int? year = null)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId();

        var orders = await repository.GetOrdersByBusinessId(businessId, null, month, year);
        var traffic = orders
            .GroupBy(o => o.Table.Place)
            .Select(g => new PlaceTrafficDto
            {
                PlaceId = g.Key.Id,
                Address = g.Key?.Address ?? "Unknown",
                CityName = g.Key?.City?.Name ?? "Unknown",
                Count = g.Count(),
                Earnings = g.Sum(o => o.TotalPrice)
            })
            .ToList();

        return traffic;
    }

    public async Task<decimal> GetTotalEarnings(DateTime? dateTime, TimeFilter? timeFilter = TimeFilter.Day, int? placeId = null)
    {
        int businessId = await CheckAuthorizationAndReturnBusinessId(placeId);

        if (dateTime == null)
            dateTime = DateTime.UtcNow;

        if (timeFilter == null)
            timeFilter = TimeFilter.Day;

        var orders = await repository.GetOrdersByTime(dateTime.Value, timeFilter.Value, businessId, placeId);
        var traffic = orders
            .Sum(o => o.TotalPrice);

        return traffic;
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
            if (!await validationService.VerifyUserPlaceAccess(placeId.Value))
                throw new UnauthorizedPlaceAccessException(placeId);
        }

        return businessId.Value;
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
