using Drizzle.Common;
using Drizzle.Common.Services;
using Drizzle.Models.Weather;
using Drizzle.Models.Weather.OpenWeatherMap;
using Drizzle.Weather.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
    private readonly string airQualityForecastApiUrl = "https://api.openweathermap.org/data/2.5/air_pollution/forecast?";
    private readonly string airQualityCurrentApiUrl = "http://api.openweathermap.org/data/2.5/air_pollution?";

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
            TimeZone = TimeUtil.GetTimeZone(latitude, longitude),
            Units = new WeatherUnitSettings(WeatherUnits.metric),
            ForecastInterval = 3,
        };

        // Group the data based on date as key
        var dailyGroup = forecastResponse.List.GroupBy(x => TimeUtil.UnixToLocalDateTime(x.Dt, result.TimeZone).Date);
        var dailyWeather = new List<DailyWeather>();
        var currentTime = TimeUtil.GetLocalTime(result.TimeZone) ?? DateTime.Now;
        var index = 0;
        // Unit ref: https://openweathermap.org/weather-data
        foreach (var day in dailyGroup)
        {
            // If data is starting from previous day then discard, can happen(?) if close to midnight.
            if (index == 0 && day.Key.Date != currentTime.Date)
            {
                index++;
                continue;
            }

            var severeDayWeather = GetMostSevereWeather(day.Select(x => OpenWeatherMapCodeToWmo(x.Weather[0].Id)));
            var currentValue = index == 0 ?
                // Select the weather closest to current time.
                day.OrderBy(t => Math.Abs((TimeUtil.UnixToLocalDateTime(t.Dt, result.TimeZone).TimeOfDay - currentTime.TimeOfDay).Ticks)).First() :
                // Pick a point in time with severe weather.
                day.First(x => OpenWeatherMapCodeToWmo(x.Weather[0].Id) == severeDayWeather);

            var weather = index == 0 ? new DailyWeather()
            {
                WeatherCode = (int)OpenWeatherMapCodeToWmo(currentResponse.Weather[0].Id),
                StartTime = TimeUtil.UnixToLocalDateTime(day.First().Dt, result.TimeZone),
                // Only current day
                Sunrise = TimeUtil.UnixToLocalDateTime(currentResponse.Sys.Sunrise, result.TimeZone),
                Sunset = TimeUtil.UnixToLocalDateTime(currentResponse.Sys.Sunset, result.TimeZone),
                TemperatureMin = day.OrderBy(x => x.Main.TempMin).First().Main.TempMin,
                TemperatureMax = day.OrderByDescending(x => x.Main.TempMax).First().Main.TempMax,
                // Not available
                //ApparentTemperatureMin =
                //ApparentTemperatureMax =
                WindSpeed = currentResponse.Wind.Speed * 3.6f, // meter/s -> km/h
                GustSpeed = currentResponse.Wind.Gust * 3.6f,
                Temperature = currentResponse.Main.Temp,
                ApparentTemperature = currentResponse.Main.FeelsLike,
                Visibility = currentValue.Visibility / 1000f, //meter -> km
                Humidity = currentResponse.Main.Humidity,
                CloudCover = currentResponse.Clouds.All,
                // Not available
                //DewPoint = 
                Pressure = currentResponse.Main.Pressure,
                WindDirection = currentResponse.Wind.Deg,
                HourlyWeatherCode = day.Select(x => (int)OpenWeatherMapCodeToWmo(x.Weather[0].Id)).ToArray(),
                HourlyTemperature = day.Select(x => x.Main.Temp).ToArray(),
                HourlyApparentTemperature = day.Select(x => x.Main.FeelsLike).ToArray(),
                HourlyVisibility = day.Select(x => x.Visibility / 1000f).ToArray(),
                HourlyHumidity = day.Select(x => x.Main.Humidity).ToArray(),
                HourlyPressure = day.Select(x => x.Main.Pressure).ToArray(),
                HourlyWindSpeed = day.Select(x => x.Wind.Speed * 3.6f).ToArray(),
                HourlyCloudCover = day.Select(x => x.Clouds.All).ToArray(),
            } : 
            new DailyWeather()
            {
                WeatherCode = (int)OpenWeatherMapCodeToWmo(currentValue.Weather[0].Id),
                StartTime = TimeUtil.UnixToLocalDateTime(day.First().Dt, result.TimeZone),
                // Only current day
                Sunrise = null,
                Sunset = null,
                TemperatureMin = day.OrderBy(x => x.Main.TempMin).First().Main.TempMin,
                TemperatureMax = day.OrderByDescending(x => x.Main.TempMax).First().Main.TempMax,
                // Not available
                //ApparentTemperatureMin =
                //ApparentTemperatureMax =
                WindSpeed = currentValue.Wind.Speed * 3.6f, // meter/s -> km/h
                GustSpeed = currentValue.Wind.Gust * 3.6f,
                Temperature = currentValue.Main.Temp,
                ApparentTemperature = currentValue.Main.FeelsLike,
                Visibility = currentValue.Visibility / 1000f, //meter -> km
                Humidity = (int)currentValue.Main.Humidity,
                CloudCover = currentValue.Clouds.All,
                // Not available
                //DewPoint = 
                Pressure = currentValue.Main.Pressure,
                WindDirection = currentValue.Wind.Deg,
                HourlyWeatherCode = day.Select(x => (int)OpenWeatherMapCodeToWmo(x.Weather[0].Id)).ToArray(),
                HourlyTemperature = day.Select(x => x.Main.Temp).ToArray(),
                HourlyApparentTemperature = day.Select(x => x.Main.FeelsLike).ToArray(),
                HourlyVisibility = day.Select(x => x.Visibility / 1000f).ToArray(),
                HourlyHumidity = day.Select(x => x.Main.Humidity).ToArray(),
                HourlyPressure = day.Select(x => x.Main.Pressure).ToArray(),
                HourlyWindSpeed = day.Select(x => x.Wind.Speed * 3.6f).ToArray(),
                HourlyCloudCover = day.Select(x => x.Clouds.All).ToArray(),
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
        var currentResponse = await GetAirQualityCurrentDataAsync(latitude, longitude);
        var forecastResponse = await GetAirQualityForecastDataAsync(latitude, longitude);

        var result = new ForecastAirQuality()
        {
            Latitude = latitude,
            Longitude = longitude,
            TimeZone = TimeUtil.GetTimeZone(latitude, longitude),
            //Units = new WeatherUnitSettings(WeatherUnits.metric),
            ForecastInterval = 1,
        };

        // Group the data based on date
        var dailyGroup = forecastResponse.List.GroupBy(x => TimeUtil.UnixToLocalDateTime(x.Dt, result.TimeZone).Date);
        var dailyAirQuality = new List<DailyAirQuality>();
        var currentTime = TimeUtil.GetLocalTime(result.TimeZone) ?? DateTime.Now;
        var index = 0;
        foreach (var day in dailyGroup)
        {
            // If data is starting from previous day then discard, can happen(?) if close to midnight.
            if (index == 0 && day.Key.Date != currentTime.Date)
            {
                index++;
                continue;
            }

            var hourlyAqi = day.Select(x => CalculateAqi(x.Components) ?? 0f);
            var airQuality = new DailyAirQuality
            {
                StartTime = TimeUtil.UnixToLocalDateTime(day.First().Dt, result.TimeZone),
                AQI = index == 0 ? CalculateAqi(currentResponse.List[0].Components) : (int)hourlyAqi.Max(),
                HourlyAQI = hourlyAqi.ToArray(),
                // Not available
                //HourlyUV = null,
                //UV = null,
            };
            dailyAirQuality.Add(airQuality);
            index++;
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
        var languageTag = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        foreach (var item in response)
        {
            string localName = null;
            // Default is English, ignore.
            if (languageTag != "en")
                item.LocalNames?.TryGetValue(languageTag, out localName);

            var tmp = new Location()
            {
                Name = localName ?? item.Name,
                DisplayName = localName ?? item.Name,
                Country = item.Country,
                Admin1 = item.State,
                Latitude = item.Lat,
                Longitude = item.Lon,
            };
            // Do not append English names if localized.
            if (localName is null)
            {
                if (item.State is not null && !item.State.Equals(item.Name, StringComparison.InvariantCulture))
                    tmp.DisplayName += $", {item.State}";
                else if (item.Country is not null && !item.Country.Equals(item.Name, StringComparison.InvariantCulture))
                    tmp.DisplayName += $", {item.Country}";
            }
            result.Add(tmp);
        }
        return result;
    }

    public async Task<IReadOnlyList<Location>> GetLocationDataAsync(float latitude, float longitude) =>
        await GetLocationDataAsync($"{latitude}, {longitude}");

    private async Task<AirQuality> GetAirQualityForecastDataAsync(float latitude, float longitude)
    {
        var url = $"{airQualityForecastApiUrl}lat={latitude}&lon={longitude}&appid={ApiKey}&units=metric";
        using var stream = await cacheService.GetFileStreamFromCacheAsync(url, true);
        return await JsonSerializer.DeserializeAsync<AirQuality>(stream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
    }

    private async Task<AirQuality> GetAirQualityCurrentDataAsync(float latitude, float longitude)
    {
        var url = $"{airQualityCurrentApiUrl}lat={latitude}&lon={longitude}&appid={ApiKey}&units=metric";
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

    private static int? CalculateAqi(AQComponents components)
    {
        var aqiPM2_5 = AirQualityUtil.GetAirQuality(Models.AQI.Particle.PM2_5, components.Pm25);
        var aqiPM10 = AirQualityUtil.GetAirQuality(Models.AQI.Particle.PM10, components.Pm10);
        var aqiNO2 = AirQualityUtil.GetAirQuality(Models.AQI.Particle.NO2, components.No2);
        var aqiSO2 = AirQualityUtil.GetAirQuality(Models.AQI.Particle.SO2, components.So2);
        var aqiCO = AirQualityUtil.GetAirQuality(Models.AQI.Particle.CO, components.Co);
        var aqiO3 = AirQualityUtil.GetAirQuality(Models.AQI.Particle.O3_8h, components.O3);

        var aqiValues = new int?[] { aqiPM2_5, aqiPM10, aqiNO2, aqiSO2, aqiCO, aqiO3 };
        return aqiValues.Max();
    }

    /// <summary>
    /// Find the most severe weather, otherwise return the most frequent one.
    /// </summary>
    private static WmoWeatherCode GetMostSevereWeather(IEnumerable<WmoWeatherCode> hourlyWeatherCodes)
    {
        var groupedWeather = hourlyWeatherCodes
            .GroupBy(code => code)
            .Select(group => new { WeatherCode = group.Key, Count = group.Count() });

        var maxSeverity = groupedWeather.Max(g => WeatherUtil.GetSeverity(g.WeatherCode));

        var mostSevereWeather = groupedWeather
            .Where(g => WeatherUtil.GetSeverity(g.WeatherCode) == maxSeverity)
            .OrderByDescending(g => g.Count)
            .First();

        return mostSevereWeather.WeatherCode;
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
            701 => WmoWeatherCode.Mist,
            711 => WmoWeatherCode.Fog,
            721 => WmoWeatherCode.Haze,
            731 => WmoWeatherCode.Dust,
            741 => WmoWeatherCode.Fog,
            751 => WmoWeatherCode.Fog,
            761 => WmoWeatherCode.Dust,
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
