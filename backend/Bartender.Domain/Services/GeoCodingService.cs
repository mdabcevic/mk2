using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Bartender.Domain.Services;

public class GeoCodingService(
    HttpClient httpClient,
    IConfiguration config
    ) : IGeoCodingService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string apiKey = config["GeoApify:ApiKey"]!;

    public async Task<(decimal Latitude, decimal Longitude)?> GetCoordinatesFromAddress(string address)
    {
        var url = $"https://api.geoapify.com/v1/geocode/search?text={Uri.EscapeDataString(address)}&apiKey={apiKey}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var features = doc.RootElement.GetProperty("features");

        if (features.GetArrayLength() == 0) return null;

        var coords = features[0].GetProperty("geometry").GetProperty("coordinates");
        decimal longitude = coords[0].GetDecimal();
        decimal latitude = coords[1].GetDecimal();

        return (latitude, longitude);
    }
}
