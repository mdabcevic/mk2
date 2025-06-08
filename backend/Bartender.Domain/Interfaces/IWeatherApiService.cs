using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IWeatherApiService
{
    Task SaveWeatherHistory(City city, DateOnly startDate, DateOnly endDate);
}
