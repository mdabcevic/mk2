
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Bartender.Domain.Services;

public class WeatherApiService(
    HttpClient httpClient,
    IRepository<WeatherData> repository,
    ILogger<WeatherApiService> logger
    ) : IWeatherApiService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task SaveWeatherHistory(City city, DateOnly startDate, DateOnly endDate)
    {
        //var url = "https://api.open-meteo.com/v1/forecast?latitude=45.81&longitude=15.98&daily=weather_code,rain_sum,showers_sum,snowfall_sum&hourly=temperature_2m,precipitation,weather_code,rain,showers,snowfall&timezone=Europe%2FBerlin&start_date=2025-03-06&end_date=2025-06-05";
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={Math.Round(city.Latitude,2)}&longitude={Math.Round(city.Longitude,2)}" +
            $"&daily=weather_code,rain_sum,showers_sum,snowfall_sum" +
            $"&hourly=temperature_2m,precipitation,weather_code,rain,showers,snowfall" +
            $"&timezone=Europe%2FBerlin&start_date={startDate.ToString("yyyy-MM-dd")}&end_date={endDate.ToString("yyyy-MM-dd")}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
        {
            logger.LogError("API response root is not an array or is empty.");
            return;
        }

        var hourly = root[0].GetProperty("hourly");
        var timeArray = hourly.GetProperty("time").EnumerateArray().ToList();
        var temperatureArray = hourly.GetProperty("temperature_2m").EnumerateArray().ToList();
        var weatherCodeArray = hourly.GetProperty("weather_code").EnumerateArray().ToList();

        if (timeArray.Count != temperatureArray.Count || timeArray.Count != weatherCodeArray.Count)
            return;

        var weatherData = new List<WeatherData>();

        for (int i = 0; i < timeArray.Count; i++) {
            if (weatherCodeArray.ElementAt(i).ValueKind != JsonValueKind.Null && timeArray.ElementAt(i).ValueKind != JsonValueKind.Null) {
                var time = DateTime.Parse(timeArray[i].GetString()!);
                var berlinZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
                var localTime = DateTime.SpecifyKind(time, DateTimeKind.Unspecified);

                if (berlinZone.IsInvalidTime(localTime))
                {
                    logger.LogWarning($"Skipping invalid local time: {localTime}");
                    continue;
                }

                time = TimeZoneInfo.ConvertTimeToUtc(localTime, berlinZone);

                var weatherCode = weatherCodeArray[i].GetInt32();
                var temperature = temperatureArray[i].ValueKind != JsonValueKind.Null
                                ? temperatureArray[i].GetDouble()
                                : (double?)null;

                weatherData.Add(new WeatherData {
                    DateTime = time,
                    CityId = city.Id,
                    Temperature = temperature,
                    WeatherType = MapWeatherCodeToCategory(weatherCode),
                });
            }
        }

        var dateTimesToSave = weatherData.Select(w => w.DateTime).ToList();

        var existingWeatherData = await repository.GetFilteredAsync(false, w => w.CityId == city.Id && dateTimesToSave.Contains(w.DateTime));

        var existingDateTimes = existingWeatherData.Select(w => w.DateTime).ToHashSet();

        var newWeatherData = weatherData
            .Where(w => !existingDateTimes.Contains(w.DateTime))
            .ToList();

        if (newWeatherData.Count > 0)
            await repository.AddMultipleAsync(newWeatherData);

    }

    public WeatherType MapWeatherCodeToCategory(int weatherCode)
    {
        //clear
        if (new[] { 0, 1, 2 }.Contains(weatherCode))
            return WeatherType.dry;
        //cloudy
        if (new[] { 3, 45, 48 }.Contains(weatherCode))
            return WeatherType.dry;
        if (new[] { 51, 53, 55, 61, 63, 65, 80, 81, 82 }.Contains(weatherCode))
            return WeatherType.rainy;
        if (new[] { 71, 73, 75, 77, 85, 86, 66, 67 }.Contains(weatherCode))
            return WeatherType.snowy;
        if (new[] { 95, 96, 99, 56, 57 }.Contains(weatherCode))
            return WeatherType.severe_weather;

        return WeatherType.unknown;
    }
}