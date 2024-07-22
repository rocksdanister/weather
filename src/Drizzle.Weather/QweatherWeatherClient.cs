using Drizzle.Common;
using Drizzle.Common.Services;
using Drizzle.Models.Weather;
using Drizzle.Models.Weather.Qweather;
using Drizzle.Weather.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Drizzle.Weather;

public class QweatherWeatherClient : IWeatherClient
{
    public string ApiKey { get; set; }
    public bool IsReverseGeocodingSupported => false;
    public bool IsApiKeyRequired => true;

    private readonly ICacheService cacheService;
    private readonly HttpClient httpClient;

    // API
    // https://dev.qweather.com/docs/api/weather/weather-hourly-forecast/
    private readonly string forecastApiUrl = "https://devapi.qweather.com/v7/weather/7d?";
    private readonly string currentApiUrl = "https://devapi.qweather.com/v7/weather/now?";
    private readonly string geocodeApiUrl = "https://geoapi.qweather.com/v2/city/lookup?";
    private readonly string hourlyForecastApiUrl = "https://devapi.qweather.com/v7/weather/24h?";
    private readonly string airQualityForecastApiUrl = "https://devapi.qweather.com/v7/air/5d?";
    private readonly string airQualityCurrentApiUrl = "https://devapi.qweather.com/v7/air/now?";

    public QweatherWeatherClient(IHttpClientFactory httpClientFactory, ICacheService cacheService, string apiKey)
    {
        this.cacheService = cacheService;
        this.httpClient = httpClientFactory.CreateClient();
        this.ApiKey = apiKey;
    }

    public async Task<ForecastWeather> QueryForecastAsync(float latitude, float longitude)
    {
        var currentResponse = await GetCurrentDataAsync(latitude, longitude);
        var forecastResponse = await GetForecastDataAsync(latitude, longitude);
        var hourlyForecastResponse = await GetHourlyForecastDataAsync(latitude, longitude);

        var result = new ForecastWeather()
        {
            // Name = !string.IsNullOrEmpty(forecastResponse.City.Name) ? $"{forecastResponse.City.Name}, {forecastResponse.City.Country}" : null,
            Latitude = latitude,
            Longitude = longitude,
            TimeZone = TimeUtil.GetTimeZone(latitude, longitude),
            Units = new WeatherUnitSettings(WeatherUnits.metric),
            ForecastInterval = 1,
        };

        // Group the data based on date as key
        var dailyGroup = forecastResponse.Daily.GroupBy(x => TimeUtil.ISO8601ToDateTime(x.FxDate).Date);
        var dailyWeather = new List<DailyWeather>();
        var currentTime = TimeUtil.GetLocalTime(result.TimeZone) ?? DateTime.Now;
        var currentHourly = hourlyForecastResponse.Hourly.Where(x => TimeUtil.ISO8601ToDateTime(x.FxTime).Date == DateTime.Today.Date);

        var index = 0;
        foreach (var day in dailyGroup)
        {
            // If data is starting from previous day then discard, can happen(?) if close to midnight.
            if (index == 0 && day.Key.Date != currentTime.Date)
            {
                index++;
                continue;
            }


            var sunriseIso8601Time = string.Format("{0}T{1}+08:00", day.First().FxDate, day.First().Sunrise);
            var sunsetIso8601Time = string.Format("{0}T{1}+08:00", day.First().FxDate, day.First().Sunset);
            var weather = index == 0 ? new DailyWeather()
            {
                WeatherCode = (int)OpenWeatherMapCodeToWmo(int.Parse(currentResponse.Now.Icon)),
                StartTime = TimeUtil.ISO8601ToDateTime(currentHourly.First().FxTime).ToLocalTime(),
               
                Sunrise =  TimeUtil.ISO8601ToDateTime(sunriseIso8601Time),
                Sunset = TimeUtil.ISO8601ToDateTime(sunsetIso8601Time),

                TemperatureMin = float.Parse(day.OrderBy(x => float.Parse(x.TempMin)).First().TempMin),
                TemperatureMax = float.Parse(day.OrderByDescending(x => float.Parse(x.TempMax)).First().TempMax),
                // Not available
                //ApparentTemperatureMin =
                //ApparentTemperatureMax =
                WindSpeed = float.Parse(currentResponse.Now.WindSpeed), //km/h
                // GustSpeed = currentResponse.Wind.Gust * 3.6f,
                Temperature = float.Parse(currentResponse.Now.Temp),
                ApparentTemperature = float.Parse(currentResponse.Now.FeelsLike),
                Visibility = float.Parse(currentResponse.Now.Vis), //km
                Humidity = int.Parse(currentResponse.Now.Humidity), 
                // Not available
                //DewPoint = 
                Pressure = float.Parse(currentResponse.Now.Pressure),
                WindDirection = float.Parse(currentResponse.Now.Wind360),
                // HourlyWeatherCode = day.Select(x => (int)OpenWeatherMapCodeToWmo(x.Weather[0].Id)).ToArray(),
                HourlyWeatherCode = currentHourly.Select(x => (int)OpenWeatherMapCodeToWmo(int.Parse(x.Icon))).ToArray(),
                HourlyTemperature = currentHourly.Select(x => float.Parse(x.Temp)).ToArray(),
                // HourlyVisibility = hourlyForecastResponse.Hourly.Select(x => float.Parse(x.Vis)).ToArray(),
                HourlyHumidity = currentHourly.Select(x => float.Parse(x.Humidity)).ToArray(),
                HourlyPressure = currentHourly.Select(x => float.Parse(x.Pressure)).ToArray(),
                HourlyWindSpeed = currentHourly.Select(x => float.Parse(x.WindSpeed)).ToArray(),
            } : 
            new DailyWeather()
            {
                WeatherCode = (int)OpenWeatherMapCodeToWmo(int.Parse(day.First().IconDay)),
                // StartTime = TimeUtil.UnixToLocalDateTime(day.First().Dt, result.TimeZone),
                StartTime = TimeUtil.ISO8601ToDateTime(day.First().FxDate),
                Sunrise = TimeUtil.ISO8601ToDateTime(sunriseIso8601Time),
                Sunset = TimeUtil.ISO8601ToDateTime(sunsetIso8601Time),
                TemperatureMin = float.Parse(day.OrderBy(x => float.Parse(x.TempMin)).First().TempMin),
                TemperatureMax = float.Parse(day.OrderByDescending(x => float.Parse(x.TempMax)).First().TempMax),
                // Not available
                //ApparentTemperatureMin =
                //ApparentTemperatureMax =
                WindSpeed = float.Parse(day.First().WindSpeedDay), // meter/s -> km/h
                // GustSpeed = currentValue.Wind.Gust * 3.6f,

                Temperature = float.Parse(day.First().TempMax),
                ApparentTemperature = float.Parse(day.First().TempMax),
                Visibility = float.Parse(day.First().Vis), //km
                Humidity = int.Parse(day.First().Humidity),
                // Not available
                //DewPoint = 

                Pressure = float.Parse(day.First().Pressure),
                WindDirection = float.Parse(day.First().Wind360Day),
                // Not available
                HourlyWeatherCode = Array.Empty<int>(),
                // HourlyWeatherCode = hourlyForecastResponse.Hourly.Select(x => (int)OpenWeatherMapCodeToWmo(int.Parse(x.Icon))).ToArray(),
                HourlyTemperature = Array.Empty<float>(),
                // HourlyVisibility = day.Select(x => x.Visibility / 1000f).ToArray(),
                // HourlyHumidity = day.Select(x => x.Main.Humidity).ToArray(),
                // HourlyPressure = day.Select(x => x.Main.Pressure).ToArray(),
                // HourlyWindSpeed = day.Select(x => x.Wind.Speed * 3.6f).ToArray(),
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
        var uvResponse = await GetForecastDataAsync(latitude, longitude);

        var result = new ForecastAirQuality()
        {
            Latitude = latitude,
            Longitude = longitude,
            TimeZone = TimeUtil.GetTimeZone(latitude, longitude),
            ForecastInterval = 1,
        };

        // Group the data based on date
        var dailyGroup = forecastResponse.List.GroupBy(x => TimeUtil.ISO8601ToDateTime(x.FxDate).Date);
        // var uvGroup = uvResponse.Daily.GroupBy(x => TimeUtil.ISO8601ToDateTime(x.FxDate).Date);
        var dailyAirQuality = new List<DailyAirQuality>();
        var currentTime = TimeUtil.GetLocalTime(result.TimeZone) ?? DateTime.Now;
        var index = 0;

        foreach (var day in dailyGroup)
        {
            if (index == 0 && day.Key.Date != currentTime.Date)
                continue;

            // var hourlyAqi = day.Select(x => CalculateAqi(x.Components) ?? 0f);
            var airQuality = new DailyAirQuality
            {
                StartTime = TimeUtil.ISO8601ToDateTime(day.First().FxDate),
                AQI = int.Parse(day.First().Aqi),
                // Not available
                //HourlyUV = null,
                //UV = float.Parse(uvGroup.Where(x => x.First().FxDate ==  ).First()),
                UV = float.Parse(uvResponse.Daily.Where(x=> x.FxDate == day.First().FxDate).First().UvIndex)
              
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
        GeocodingApiResponse response;

        response = await GetGeocodingDataAsync(place, 5);

        var result = new List<Location>();

        if (response.location == null || !response.location.Any()) {
            return result;
        }

        foreach (var item in response.location)
        {
            string localName = null;

            var tmp = new Location()
            {
                Name = localName ?? item.Name,
                DisplayName = localName ?? item.Name,
                Country = item.Country,
                Admin1 = item.Adm1,
                Latitude = float.Parse(item.Lat),
                Longitude = float.Parse(item.Lon),
            };

            result.Add(tmp);
        }
        return result;
    }


    public async Task<IReadOnlyList<Location>> GetLocationDataAsync(float latitude, float longitude) =>
        await GetLocationDataAsync($"{latitude},{longitude}");

    private async Task<AirQualityForecast> GetAirQualityForecastDataAsync(float latitude, float longitude)
    {
        var url = $"{airQualityForecastApiUrl}location={longitude},{latitude}&key={ApiKey}";
        using var stream = await cacheService.GetFileStreamFromCacheAsync(url, true);
        return await DeserializeCompressed<AirQualityForecast>(stream);
    }

    private async Task<AirQualityCurrent> GetAirQualityCurrentDataAsync(float latitude, float longitude)
    {
        var url = $"{airQualityCurrentApiUrl}location={longitude},{latitude}&key={ApiKey}";
        using var stream = await cacheService.GetFileStreamFromCacheAsync(url, true);
        return await DeserializeCompressed<AirQualityCurrent>(stream);
    }

    private async Task<Current> GetCurrentDataAsync(float latitude, float longitude)
    {
        var url = $"{currentApiUrl}location={longitude},{latitude}&key={ApiKey}&unit=m";
        using var stream = await cacheService.GetFileStreamFromCacheAsync(url, true);
        return await DeserializeCompressed<Current>(stream);
    }

    private async Task<Forecast> GetForecastDataAsync(float latitude, float longitude)
    {
        var url = $"{forecastApiUrl}location={longitude},{latitude}&key={ApiKey}&unit=m";
        using var stream = await cacheService.GetFileStreamFromCacheAsync(url, true);
        return await DeserializeCompressed<Forecast>(stream);
    }

    private async Task<HourlyForecast> GetHourlyForecastDataAsync(float latitude, float longitude)
    {
        var url = $"{hourlyForecastApiUrl}location={longitude},{latitude}&key={ApiKey}&unit=m";
        using var stream = await cacheService.GetFileStreamFromCacheAsync(url, true);
        return await DeserializeCompressed<HourlyForecast>(stream);
    }

    private async Task<GeocodingApiResponse> GetGeocodingDataAsync(string place, int limit)
    {
        using var response = await httpClient.GetAsync($"{geocodeApiUrl}location={place}&number={limit}&key={ApiKey}");
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        return await DeserializeCompressed<GeocodingApiResponse>(stream);

    }
    public async Task<T> DeserializeCompressed<T>(Stream stream, Newtonsoft.Json.JsonSerializerSettings settings = null)
    {
        using (var compressor = new GZipStream(stream, CompressionMode.Decompress))
        using (var reader = new StreamReader(compressor))
        {
            return await JsonSerializer.DeserializeAsync<T>(reader.BaseStream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }); ;
        }
    }

    // Ref:
    // https://dev.qweather.com/docs/resource/icons/
    private static WmoWeatherCode OpenWeatherMapCodeToWmo(int code)
    {
        return code switch
        {
            // Rain
            300 => WmoWeatherCode.SlightRainShowers,
            301 => WmoWeatherCode.ModerateRainShowers,
            //  302-304 thunder shower
            302 => WmoWeatherCode.Thunderstorm,
            303 => WmoWeatherCode.Thunderstorm,
            304 => WmoWeatherCode.Thunderstorm,

            305 => WmoWeatherCode.SlightRain,
            306 => WmoWeatherCode.ModerateRain,
            307 => WmoWeatherCode.HeavyRain,

            308 => WmoWeatherCode.ViolentRainShowers,
            309 => WmoWeatherCode.LightDrizzle,
            // 310-312  rain storm 
            310 => WmoWeatherCode.ModerateRain,
            311 => WmoWeatherCode.HeavyRain,
            312 => WmoWeatherCode.HeavyRain,

            313 => WmoWeatherCode.HeavyFreezingRain,
            // 314-318  rain storm
            314 => WmoWeatherCode.SlightRain,
            315 => WmoWeatherCode.ModerateRain,
            316 => WmoWeatherCode.HeavyRain,
            317 => WmoWeatherCode.HeavyRain,
            318 => WmoWeatherCode.HeavyRain,

            350 => WmoWeatherCode.SlightRainShowers,
            351 => WmoWeatherCode.ModerateRainShowers,
            399 => WmoWeatherCode.ModerateRain,


            400 => WmoWeatherCode.SlightSnowFall,
            401 => WmoWeatherCode.ModerateSnowFall,
            402 => WmoWeatherCode.HeavySnowFall,
            403 => WmoWeatherCode.HeavySnowShowers,
            404 => WmoWeatherCode.SnowGrains,
            405 => WmoWeatherCode.SnowGrains,
            406 => WmoWeatherCode.SnowGrains,
            407 => WmoWeatherCode.SlightSnowShowers,
            408 => WmoWeatherCode.SlightSnowFall,
            409 => WmoWeatherCode.ModerateSnowFall,
            410 => WmoWeatherCode.HeavySnowFall,

            456 => WmoWeatherCode.SnowGrains,
            457 => WmoWeatherCode.SlightSnowShowers,
            499 => WmoWeatherCode.SlightSnowFall,


            500 => WmoWeatherCode.Mist,
            501 => WmoWeatherCode.Fog,
            502 => WmoWeatherCode.Haze,
            503 => WmoWeatherCode.Dust,
            504 => WmoWeatherCode.Dust,

            507 => WmoWeatherCode.Dust,
            508 => WmoWeatherCode.Dust,
            509 => WmoWeatherCode.Fog,
            510 => WmoWeatherCode.Fog,

            511 => WmoWeatherCode.Haze,
            512 => WmoWeatherCode.Haze,
            513 => WmoWeatherCode.Haze,
            514 => WmoWeatherCode.Haze,
            515 => WmoWeatherCode.Haze,

            // Cloud
            100 => WmoWeatherCode.ClearSky,
            101 => WmoWeatherCode.PartlyCloudy,
            102 => WmoWeatherCode.MainlyClear,
            103 => WmoWeatherCode.PartlyCloudy,
            104 => WmoWeatherCode.Overcast,
            150 => WmoWeatherCode.ClearSky,
            151 => WmoWeatherCode.PartlyCloudy,
            152 => WmoWeatherCode.MainlyClear,
            153 => WmoWeatherCode.PartlyCloudy,

         

            900 => WmoWeatherCode.ClearSky,
            901 => WmoWeatherCode.Overcast,
            999 => WmoWeatherCode.ClearSky,
            _ => throw new NotImplementedException(),
        };
    }
}
