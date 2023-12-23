using Drizzle.Common;
using Drizzle.Common.Services;
using Drizzle.Models.Weather;
using Drizzle.Models.Weather.OpenWeatherMap;
using Drizzle.Weather.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Drizzle.Weather;

public class OpenWeatherMapWeatherClient : IWeatherClient
{
    public string ApiKey { get; set; }
    public bool IsReverseGeocodingSupported => true;
    public bool IsApiKeyRequired => true;

    private readonly ICacheService cacheService;
    private readonly HttpClient httpClient;

    // API Ref:
    // https://openweathermap.org/forecast5
    // https://openweathermap.org/current
    // https://openweathermap.org/api/air-pollution
    // https://openweathermap.org/api/geocoding-api
    private readonly string forecastApiUrl = "https://api.openweathermap.org/data/2.5/forecast?";
    private readonly string currentApiUrl = "https://api.openweathermap.org/data/2.5/weather?";
    private readonly string geocodeApiUrl = "https://api.openweathermap.org/geo/1.0/direct?";
    private readonly string reverseGeocodeApiUrl = "http://api.openweathermap.org/geo/1.0/reverse?";
    private readonly string airQualityApiUrl = "https://api.openweathermap.org/data/2.5/air_pollution/forecast?";

    public OpenWeatherMapWeatherClient(IHttpClientFactory httpClientFactory, ICacheService cacheService, string apiKey)
    {
        this.cacheService = cacheService;
        this.httpClient = httpClientFactory.CreateClient();
        this.ApiKey = apiKey;
    }

    public async Task<ForecastWeather> QueryForecastAsync(float latitude, float longitude)
    {
        var currentResponse = await GetCurrentDataAsync(latitude, longitude);
        var forecastResponse = await GetForecastDataAsync(latitude, longitude);

        var result = new ForecastWeather()
        {
            Name = !string.IsNullOrEmpty(forecastResponse.City.Name) ? $"{forecastResponse.City.Name}, {forecastResponse.City.Country}" : null,
            Latitude = latitude,
            Longitude = longitude,
            TimeZone = WeatherUtil.GetTimeZone(latitude, longitude),
            Units = new ForecastWeatherUnits(WeatherUnits.metric),
            ForecastInterval = 3,
        };

        // Group the data based on date as key
        var dailyGroup = forecastResponse.List.GroupBy(x => WeatherUtil.UnixToLocalDateTime(x.Dt, result.TimeZone).Date);
        var dailyWeather = new List<DailyWeather>();
        var index = 0;
        // Unit ref: https://openweathermap.org/weather-data
        foreach (var day in dailyGroup)
        {
            var selection = GetValueCloseToTime(day, result.TimeZone);
            var weather = new DailyWeather()
            {
                WeatherCode = (int)OpenWeatherMapCodeToWmo(selection.Weather[0].Id),
                Date = WeatherUtil.UnixToLocalDateTime(day.First().Dt, result.TimeZone),
                // Only current day
                Sunrise = index == 0 ? WeatherUtil.UnixToLocalDateTime(currentResponse.Sys.Sunrise, result.TimeZone) : null,
                Sunset = index == 0 ? WeatherUtil.UnixToLocalDateTime(currentResponse.Sys.Sunset, result.TimeZone) : null,
                TemperatureMin = day.OrderBy(x => x.Main.TempMin).First().Main.TempMin,
                TemperatureMax = day.OrderByDescending(x => x.Main.TempMax).First().Main.TempMax,
                // Not available
                //ApparentTemperatureMin =
                //ApparentTemperatureMax =
                WindSpeed = index == 0 ? currentResponse.Wind.Speed * 3.6f : selection.Wind.Speed * 3.6f, // meter/s -> km/h
                GustSpeed = index == 0 ? currentResponse.Wind.Gust * 3.6f : selection.Wind.Gust * 3.6f,
                Temperature = index == 0 ? currentResponse.Main.Temp : selection.Main.Temp,
                ApparentTemperature = index == 0 ? currentResponse.Main.FeelsLike : selection.Main.FeelsLike,
                Visibility = index == 0 ? currentResponse.Main.FeelsLike / 1000f : selection.Visibility / 1000f, //meter -> km
                Humidity = index == 0 ? (int)currentResponse.Main.Humidity : (int)selection.Main.Humidity,
                // Not available
                //DewPoint = 
                Pressure = index == 0 ? currentResponse.Main.Pressure : selection.Main.Pressure,
                WindDirection = index == 0 ? WeatherUtil.MeteorologicalDegreeToRegular(currentResponse.Wind.Deg) : WeatherUtil.MeteorologicalDegreeToRegular(selection.Wind.Deg),
                HourlyWeatherCode = day.Select(x => (int)OpenWeatherMapCodeToWmo(x.Weather[0].Id)).ToArray(),
                HourlyTemperature = day.Select(x => x.Main.Temp).ToArray(),
                HourlyVisibility = day.Select(x => x.Visibility).ToArray(),
                HourlyHumidity = day.Select(x => x.Main.Humidity).ToArray(),
                HourlyPressure = day.Select(x => x.Main.Pressure).ToArray(),
                HourlyWindSpeed = day.Select(x => x.Wind.Speed).ToArray(),
            };
            dailyWeather.Add(weather);
            index++;
        }
        result.FetchTime = cacheService.LastAccessTime;
        result.Daily = dailyWeather;
        return result;
    }

    public async Task<ForecastAirQuality> QueryAirQualityAsync(float latitude, float longitude)
    {
        var response = await GetAirQualityForecastDataAsync(latitude, longitude);

        var result = new ForecastAirQuality()
        {
            Latitude = latitude,
            Longitude = longitude,
            TimeZone = WeatherUtil.GetTimeZone(latitude, longitude),
            Units = new ForecastAirQualityUnits(WeatherUnits.metric),
            ForecastInterval = 3,
        };

        // Group the data based on date
        var dailyGroup = response.List.GroupBy(x => WeatherUtil.UnixToLocalDateTime(x.Dt, result.TimeZone).Date);
        var dailyAirQuality = new List<DailyAirQuality>();
        foreach (var day in dailyGroup)
        {
            var selection = GetValueCloseToTime(day, result.TimeZone);
            var airQuality = new DailyAirQuality
            {
                // TODO: Convert OpenWeather to US AQI
                // https://www.airnow.gov/sites/default/files/2020-05/aqi-technical-assistance-document-sept2018.pdf
                // https://openweathermap.org/api/air-pollution
                // https://openweathermap.org/air-pollution-index-levels
                AQI = null,//selection.Main.Aqi,
                Date = WeatherUtil.UnixToLocalDateTime(selection.Dt, result.TimeZone),
                //HourlyAQI = day.Select(x => (float)x.Main.Aqi).ToArray(),
                // Not available
                //HourlyUV = null,
                //UV = null,
            };
            dailyAirQuality.Add(airQuality);
        }
        result.FetchTime = cacheService.LastAccessTime;
        result.Daily = dailyAirQuality;
        return result;
    }

    public async Task<IReadOnlyList<Location>> GetLocationDataAsync(string place)
    {
        LocationData[] response;
        var split = place.Split(',');
        if (split.Count() == 2 && float.TryParse(split[0], out var latitude) && float.TryParse(split[1], out var longitude))
            response = await GetReverseGeocodingDataAsync(latitude, longitude, 5);
        else
            response = await GetGeocodingDataAsync(place, 5);

        var result = new List<Location>();
        foreach (var item in response)
        {
            var tmp = new Location()
            {
                Name = item.Name,
                Country = item.Country,
                Admin1 = item.State,
                Latitude = item.Lat,
                Longitude = item.Lon,
            };
            tmp.DisplayName = item.Name;
            if (item.State is not null && !item.State.Equals(item.Name, StringComparison.InvariantCulture))
                tmp.DisplayName += $", {item.State}";
            else if (item.Country is not null && !item.Country.Equals(item.Name, StringComparison.InvariantCulture))
                tmp.DisplayName += $", {item.Country}";
            result.Add(tmp);
        }
        return result;
    }

    public async Task<IReadOnlyList<Location>> GetLocationDataAsync(float latitude, float longitude) =>
        await GetLocationDataAsync($"{latitude}, {longitude}");

    private async Task<AirQuality> GetAirQualityForecastDataAsync(float latitude, float longitude)
    {
        var url = $"{airQualityApiUrl}lat={latitude}&lon={longitude}&appid={ApiKey}&units=metric";
        using var stream = await cacheService.GetFileStreamFromCacheAsync(url, true);
        return await JsonSerializer.DeserializeAsync<AirQuality>(stream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
    }

    private async Task<Current> GetCurrentDataAsync(float latitude, float longitude)
    {
        var url = $"{currentApiUrl}lat={latitude}&lon={longitude}&appid={ApiKey}&units=metric";
        using var stream = await cacheService.GetFileStreamFromCacheAsync(url, true);
        return await JsonSerializer.DeserializeAsync<Current>(stream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
    }

    private async Task<Forecast> GetForecastDataAsync(float latitude, float longitude)
    {
        var url = $"{forecastApiUrl}lat={latitude}&lon={longitude}&appid={ApiKey}&units=metric";
        using var stream = await cacheService.GetFileStreamFromCacheAsync(url, true);
        return await JsonSerializer.DeserializeAsync<Forecast>(stream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
    }

    private async Task<LocationData[]> GetGeocodingDataAsync(string place, int limit)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"{geocodeApiUrl}q={place}&limit={limit}&appid={ApiKey}");
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<LocationData[]>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
    }

    private async Task<LocationData[]> GetReverseGeocodingDataAsync(float latitude, float longitude, int limit)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"{reverseGeocodeApiUrl}lat={latitude}&lon={longitude}&limit={limit}&appid={ApiKey}");
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<LocationData[]>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
    }

    private static ForecastList GetValueCloseToTime(IGrouping<DateTime, ForecastList> day, string timezone)
    {
        var currentTime = WeatherUtil.GetLocalTime(timezone)?.TimeOfDay ?? DateTime.Now.TimeOfDay;
        return day.OrderBy(t => Math.Abs((WeatherUtil.UnixToLocalDateTime(t.Dt, timezone).TimeOfDay - currentTime).Ticks)).First();
    }

    private static AQList GetValueCloseToTime(IGrouping<DateTime, AQList> day, string timezone)
    {
        var currentTime = WeatherUtil.GetLocalTime(timezone)?.TimeOfDay ?? DateTime.Now.TimeOfDay;
        return day.OrderBy(t => Math.Abs((WeatherUtil.UnixToLocalDateTime(t.Dt, timezone).TimeOfDay - currentTime).Ticks)).First();
    }

    // Ref:
    // https://openweathermap.org/weather-conditions#Weather-Condition-Codes-2
    // https://gist.github.com/stellasphere/9490c195ed2b53c707087c8c2db4ec0c
    private static WmoWeatherCode OpenWeatherMapCodeToWmo(int code)
    {
        return code switch
        {
            // Group 2xx: Thunderstorm
            200 => WmoWeatherCode.Thunderstorm,
            201 => WmoWeatherCode.Thunderstorm,
            202 => WmoWeatherCode.Thunderstorm,
            210 => WmoWeatherCode.Thunderstorm,
            211 => WmoWeatherCode.Thunderstorm,
            212 => WmoWeatherCode.Thunderstorm,
            221 => WmoWeatherCode.Thunderstorm,
            230 => WmoWeatherCode.Thunderstorm,
            231 => WmoWeatherCode.Thunderstorm,
            232 => WmoWeatherCode.Thunderstorm,
            // Group 3xx: Drizzle
            300 => WmoWeatherCode.LightDrizzle,
            301 => WmoWeatherCode.ModerateDrizzle,
            302 => WmoWeatherCode.DenseDrizzle,
            310 => WmoWeatherCode.LightDrizzle,
            311 => WmoWeatherCode.ModerateDrizzle,
            312 => WmoWeatherCode.DenseDrizzle,
            313 => WmoWeatherCode.ModerateDrizzle,
            314 => WmoWeatherCode.ModerateDrizzle,
            321 => WmoWeatherCode.ModerateDrizzle,
            // Group 5xx: Rain
            500 => WmoWeatherCode.SlightRain,
            501 => WmoWeatherCode.ModerateRain,
            502 => WmoWeatherCode.HeavyRain,
            503 => WmoWeatherCode.HeavyRain,
            504 => WmoWeatherCode.HeavyRain,
            511 => WmoWeatherCode.HeavyRain,
            520 => WmoWeatherCode.SlightRainShowers,
            521 => WmoWeatherCode.ModerateRainShowers,
            522 => WmoWeatherCode.SlightRainShowers,
            531 => WmoWeatherCode.ModerateRain,
            // Group 6xx: Snow
            600 => WmoWeatherCode.SlightSnowFall,
            601 => WmoWeatherCode.ModerateSnowFall,
            602 => WmoWeatherCode.HeavySnowFall,
            611 => WmoWeatherCode.SlightSnowFall,
            612 => WmoWeatherCode.ModerateSnowFall,
            613 => WmoWeatherCode.ModerateSnowFall,
            615 => WmoWeatherCode.ModerateSnowFall,
            616 => WmoWeatherCode.ModerateSnowFall,
            620 => WmoWeatherCode.SlightSnowShowers,
            621 => WmoWeatherCode.SlightSnowShowers,
            622 => WmoWeatherCode.HeavySnowShowers,
            // Group 7xx: Atmosphere
            701 => WmoWeatherCode.Fog,
            711 => WmoWeatherCode.Fog,
            721 => WmoWeatherCode.Fog,
            731 => WmoWeatherCode.Fog,
            741 => WmoWeatherCode.Fog,
            751 => WmoWeatherCode.Fog,
            761 => WmoWeatherCode.Fog,
            762 => WmoWeatherCode.Fog,
            771 => WmoWeatherCode.Fog,
            781 => WmoWeatherCode.Fog,
            // Group 800: Clear
            800 => WmoWeatherCode.ClearSky,
            // Group 80x: Clouds
            801 => WmoWeatherCode.PartlyCloudy,
            802 => WmoWeatherCode.PartlyCloudy,
            803 => WmoWeatherCode.Overcast,
            804 => WmoWeatherCode.Overcast,
            _ => throw new NotImplementedException(),
        };
    }
}
