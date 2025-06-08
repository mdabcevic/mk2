
namespace Bartender.Domain.Interfaces;

public interface IGeoCodingService
{
    Task<(decimal Latitude, decimal Longitude)?> GetCoordinatesFromAddress(string address);
}
