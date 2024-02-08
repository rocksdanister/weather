using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Drizzle.Models.Weather;
using Drizzle.Common.Services;
using System.IO;
using Microsoft.Extensions.Logging;
using Drizzle.Models.Weather.OpenMeteo;
using Drizzle.Weather.Helpers;

namespace Drizzle.Weather;

// Based on: https://github.com/AlienDwarf/open-meteo-dotnet
// MIT License Copyright(c) 2022 AlienDwarf
public class OpenMeteoWeatherClient : IWeatherClient
{
    // Not required for this client
    public string ApiKey { get; set; }
    public bool IsReverseGeocodingSupported => false;
    public bool IsApiKeyRequired => false;

    // API Ref:
    // https://open-meteo.com/en/docs
    // https://open-meteo.com/en/docs/geocoding-api
    // https://open-meteo.com/en/docs/air-quality-api
    private readonly string weatherApiUrl = "https://api.open-meteo.com/v1/forecast";
    private readonly string geocodeApiUrl = "https://geocoding-api.open-meteo.com/v1/search";
    private readonly string airQualityApiUrl = "https://air-quality-api.open-meteo.com/v1/air-quality";

    private readonly HttpClient httpClient;
    private readonly ICacheService cacheService;
    private readonly ILogger logger;

    public OpenMeteoWeatherClient(IHttpClientFactory httpClientFactory,
        ICacheService cacheService,
        ILogger<OpenMeteoWeatherClient> logger)
    {
        httpClient = httpClientFactory.CreateClient();
        this.cacheService = cacheService;
        this.logger = logger;
    }

    public async Task<ForecastWeather> QueryForecastAsync(float latitude, float longitude)
    {
        var options = new WeatherForecastOptions
        {
            Timezone = "auto",
            Latitude = latitude,
            Longitude = longitude,
            Current_Weather = true,
            // Deserialize error?
            //Timeformat = TimeformatType.unixtime,
            Daily = new DailyOptions()
            {
                DailyOptionsParameter.sunrise,
                DailyOptionsParameter.sunset,
                DailyOptionsParameter.weathercode,
                DailyOptionsParameter.temperature_2m_max,
                DailyOptionsParameter.temperature_2m_min,
                DailyOptionsParameter.apparent_temperature_max,
                DailyOptionsParameter.apparent_temperature_min,
                DailyOptionsParameter.windspeed_10m_max,
                DailyOptionsParameter.winddirection_10m_dominant,
                DailyOptionsParameter.windgusts_10m_max
            },
            Hourly = new HourlyOptions()
            {
                HourlyOptionsParameter.temperature_2m,
                HourlyOptionsParameter.relativehumidity_2m,
                HourlyOptionsParameter.visibility,
                HourlyOptionsParameter.dewpoint_2m,
                HourlyOptionsParameter.pressure_msl,
                HourlyOptionsParameter.windspeed_10m,
                HourlyOptionsParameter.winddirection_10m,
                HourlyOptionsParameter.apparent_temperature,
                HourlyOptionsParameter.weathercode,
            },
        };
        var response = await GetWeatherForecastAsync(options);

        var result = new ForecastWeather()
        {
            Latitude = latitude,
            Longitude = longitude,
            TimeZone = response.Timezone,
            Units = new ForecastWeatherUnits(WeatherUnits.metric),
            ForecastInterval = 1,
        };

        var index = 0;
        var dailyWeather = new List<DailyWeather>();
        var parsedResponse = GroupByDate(response);
        var currentTime = TimeUtil.GetLocalTime(response.Timezone) ?? DateTime.Now;
        foreach (var item in parsedResponse)
        {
            var date = item.Key;
            var value = item.Value;
            var currentHourValue = value.Hourly.OrderBy(x => Math.Abs((x.Time.TimeOfDay - currentTime.TimeOfDay).Ticks)).First();

            // If data is starting from previous day then discard, can happen(?) if close to midnight.
            if (index == 0 && date.Date != currentTime.Date)
                continue;

            var weather = new DailyWeather
            {
                // Hourly weather for today, otherwise severe weather for the day
                WeatherCode = index == 0 ? (int)currentHourValue.WeatherCode : (int)value.Daily.WeatherCode,
                Date = date,
                Sunrise = value.Daily?.Sunrise,
                Sunset = value.Daily?.Sunset,
                TemperatureMin = value.Daily?.TemperatureMin,
                TemperatureMax = value.Daily?.TemperatureMax,
                ApparentTemperatureMin = value.Daily?.ApparentTemperatureMin,
                ApparentTemperatureMax = value.Daily?.ApparentTemperatureMax,
                WindSpeed = value.Daily?.WindSpeed,
                GustSpeed = value.Daily?.GustSpeed,
                Temperature = currentHourValue.Temperature,
                ApparentTemperature = currentHourValue.ApparentTemperature,
                Visibility = currentHourValue.Visibility / 1000f, //meter -> km
                Humidity = (int)currentHourValue.Humidity,
                DewPoint = currentHourValue.DewPoint,
                Pressure = currentHourValue.Pressure,
                WindDirection = value.Daily?.WindDirection,
                HourlyWeatherCode = value.Hourly.Select(x => x.WeatherCode is null ? 0 : (int)x.WeatherCode).ToArray(),
                HourlyTemperature = value.Hourly.Select(x => x.Temperature is null ? 0 : (float)x.Temperature).ToArray(),
                HourlyVisibility = value.Hourly.Select(x => x.Temperature is null ? 0 : (float)x.Visibility).ToArray(),
                HourlyHumidity = value.Hourly.Select(x => x.Temperature is null ? 0 : (float)x.Humidity).ToArray(),
                HourlyPressure = value.Hourly.Select(x => x.Temperature is null ? 0 : (float)x.Pressure).ToArray(),
                HourlyWindSpeed = value.Hourly.Select(x => x.Temperature is null ? 0 : (float)x.WindSpeed).ToArray(),
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
        var options = new AirQualityOptions
        {
            Timezone = "auto",
            Latitude = latitude,
            Longitude = longitude,
            Hourly = new()
            {
                AirQualityOptions.HourlyOptionsParameter.uv_index,
                //AirQualityOptions.HourlyOptionsParameter.uv_index_clear_sky,
                //AirQualityOptions.HourlyOptionsParameter.us_aqi_pm2_5,
                //AirQualityOptions.HourlyOptionsParameter.us_aqi_pm10,
                AirQualityOptions.HourlyOptionsParameter.us_aqi,

            }
        };
        var response = await GetAirQualityAsync(options);

        var result = new ForecastAirQuality()
        {
            Latitude = latitude,
            Longitude = longitude,
            TimeZone = response.Timezone,
            Units = new ForecastAirQualityUnits(WeatherUnits.metric),
            ForecastInterval = 1,
        };

        var index = 0;
        var dailyAirQuality = new List<DailyAirQuality>();
        var parsedResponse = GroupByDate(response);
        var currentTime = TimeUtil.GetLocalTime(response.Timezone) ?? DateTime.Now;
        foreach (var item in parsedResponse)
        {
            var date = item.Key;
            var value = item.Value;
            var currentHourValue = value.Hourly.OrderBy(x => Math.Abs((x.Time.TimeOfDay - currentTime.TimeOfDay).Ticks)).First();

            // If data is starting from previous day then discard, can happen(?) if close to midnight.
            if (index == 0 && date.Date != currentTime.Date)
                continue;

            var airQuality = new DailyAirQuality
            {
                Date = date,
                UV = currentHourValue.UV,
                AQI = (int)(currentHourValue.AQI ?? 0),
                HourlyAQI = value.Hourly.Select(x => x.AQI is null ? 0 : (float)x.AQI).ToArray(),
                HourlyUV = value.Hourly.Select(x => x.UV is null ? 0 : (float)x.UV).ToArray(),
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
        //Issue: https://github.com/open-meteo/geocoding-api/issues/12
        var searchQuery = place;
        int index = place.IndexOf(",");
        if (index >= 0)
            searchQuery = place.Substring(0, index);

        GeocodingOptions geocodingOptions = new(searchQuery, 25);
        var response = await GetGeocodingDataAsync(geocodingOptions);

        var result = new List<Location>();
        if (response?.Locations is not null)
        {
            foreach (var item in response.Locations)
            {
                var tmp = new Location()
                {
                    Name = item.Name,
                    Country = item.Country,
                    Admin1 = item.Admin1,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                };
                tmp.DisplayName = item.Name;
                if (item.Admin1 is not null && !item.Admin1.Equals(item.Name, StringComparison.InvariantCulture))
                    tmp.DisplayName += $", {item.Admin1}";
                else if (item.Country is not null && !item.Country.Equals(item.Name, StringComparison.InvariantCulture))
                    tmp.DisplayName += $", {item.Country}";

                result.Add(tmp);
            }
        }

        // Show exact match if possible (see issue above)
        if (index >= 0)
            result = result.OrderByDescending(x => x.DisplayName.StartsWith(place, StringComparison.InvariantCultureIgnoreCase)).ToList();

        return result;
    }

    public Task<IReadOnlyList<Location>> GetLocationDataAsync(float latitude, float longitude)
    {
        throw new NotSupportedException();
    }

    private async Task<WeatherForecast?> GetWeatherForecastAsync(WeatherForecastOptions options)
    {
        logger.LogInformation("Fetching weather forecast..");
        var url = MergeUrlWithOptions(weatherApiUrl, options);
        using var stream = await cacheService.GetFileStreamFromCacheAsync(url, true);
        logger.LogInformation($"Cached weather forecast {cacheService.LastAccessTime}.");
        return await JsonSerializer.DeserializeAsync<WeatherForecast>(stream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
    }

    private async Task<AirQuality?> GetAirQualityAsync(AirQualityOptions options)
    {
        logger.LogInformation("Fetching airquality forecast..");
        var url = MergeUrlWithOptions(airQualityApiUrl, options);
        using var stream = await cacheService.GetFileStreamFromCacheAsync(url, true);
        logger.LogInformation($"Cached airquality forecast {cacheService.LastAccessTime}.");
        return await JsonSerializer.DeserializeAsync<AirQuality>(stream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
    }

    private async Task<GeocodingApiResponse?> GetGeocodingDataAsync(GeocodingOptions options)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(MergeUrlWithOptions(geocodeApiUrl, options));
        response.EnsureSuccessStatusCode();

        GeocodingApiResponse? geocodingData = await JsonSerializer.DeserializeAsync<GeocodingApiResponse>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        return geocodingData;
    }

    /// <summary>
    /// Combines a given url with an options object to create a url for GET requests
    /// </summary>
    /// <returns>url+queryString</returns>
    private string MergeUrlWithOptions(string url, GeocodingOptions options)
    {
        if (options == null) return url;

        UriBuilder uri = new UriBuilder(url);
        bool isFirstParam = false;

        // If no query given, add '?' to start the query string
        if (uri.Query == string.Empty)
        {
            uri.Query = "?";

            // isFirstParam becomes true because the query string is new
            isFirstParam = true;
        }

        // Now we check every property and set the value, if neccessary
        if (isFirstParam)
            uri.Query += "name=" + options.Name;
        else
            uri.Query += "&name=" + options.Name;

        if (options.Count > 0)
            uri.Query += "&count=" + options.Count;

        if (options.Format != string.Empty)
            uri.Query += "&format=" + options.Format;

        if (options.Language != string.Empty)
            uri.Query += "&language=" + options.Language;

        return uri.ToString();
    }

    private string MergeUrlWithOptions(string url, WeatherForecastOptions? options)
    {
        if (options == null) return url;

        UriBuilder uri = new UriBuilder(url);
        bool isFirstParam = false;

        // If no query given, add '?' to start the query string
        if (uri.Query == string.Empty)
        {
            uri.Query = "?";

            // isFirstParam becomes true because the query string is new
            isFirstParam = true;
        }

        // Add the properties

        // Begin with Latitude and Longitude since they're required
        if (isFirstParam)
            uri.Query += "latitude=" + options.Latitude.ToString(CultureInfo.InvariantCulture);
        else
            uri.Query += "&latitude=" + options.Latitude.ToString(CultureInfo.InvariantCulture);

        uri.Query += "&longitude=" + options.Longitude.ToString(CultureInfo.InvariantCulture);

        uri.Query += "&temperature_unit=" + options.Temperature_Unit.ToString();
        uri.Query += "&windspeed_unit=" + options.Windspeed_Unit.ToString();
        uri.Query += "&precipitation_unit=" + options.Precipitation_Unit.ToString();
        if (options.Timezone != string.Empty)
            uri.Query += "&timezone=" + options.Timezone;

        uri.Query += "&current_weather=" + options.Current_Weather;

        uri.Query += "&timeformat=" + options.Timeformat.ToString();

        uri.Query += "&past_days=" + options.Past_Days;

        if (options.Start_date != string.Empty)
            uri.Query += "&start_date=" + options.Start_date;
        if (options.End_date != string.Empty)
            uri.Query += "&end_date=" + options.End_date;

        // Now we iterate through hourly and daily

        // Hourly
        if (options.Hourly.Count > 0)
        {
            bool firstHourlyElement = true;
            uri.Query += "&hourly=";

            foreach (var option in options.Hourly)
            {
                if (firstHourlyElement)
                {
                    uri.Query += option.ToString();
                    firstHourlyElement = false;
                }
                else
                {
                    uri.Query += "," + option.ToString();
                }
            }
        }

        // Daily
        if (options.Daily.Count > 0)
        {
            bool firstDailyElement = true;
            uri.Query += "&daily=";
            foreach (var option in options.Daily)
            {
                if (firstDailyElement)
                {
                    uri.Query += option.ToString();
                    firstDailyElement = false;
                }
                else
                {
                    uri.Query += "," + option.ToString();
                }
            }
        }

        // 0.2.0 Weather models
        // cell_selection
        uri.Query += "&cell_selection=" + options.Cell_Selection;

        // Models
        if (options.Models.Count > 0)
        {
            bool firstModelsElement = true;
            uri.Query += "&models=";
            foreach (var option in options.Models)
            {
                if (firstModelsElement)
                {
                    uri.Query += option.ToString();
                    firstModelsElement = false;
                }
                else
                {
                    uri.Query += "," + option.ToString();
                }
            }
        }

        return uri.ToString();
    }

    /// <summary>
    /// Combines a given url with an options object to create a url for GET requests
    /// </summary>
    /// <returns>url+queryString</returns>
    private string MergeUrlWithOptions(string url, AirQualityOptions options)
    {
        if (options == null) return url;

        UriBuilder uri = new UriBuilder(url);
        bool isFirstParam = false;

        // If no query given, add '?' to start the query string
        if (uri.Query == string.Empty)
        {
            uri.Query = "?";

            // isFirstParam becomes true because the query string is new
            isFirstParam = true;
        }

        // Now we check every property and set the value, if neccessary
        if (isFirstParam)
            uri.Query += "latitude=" + options.Latitude.ToString(CultureInfo.InvariantCulture);
        else
            uri.Query += "&latitude=" + options.Latitude.ToString(CultureInfo.InvariantCulture);

        uri.Query += "&longitude=" + options.Longitude.ToString(CultureInfo.InvariantCulture);

        if (options.Domains != string.Empty)
            uri.Query += "&domains=" + options.Domains;

        if (options.Timeformat != string.Empty)
            uri.Query += "&timeformat=" + options.Timeformat;

        if (options.Timezone != string.Empty)
            uri.Query += "&timezone=" + options.Timezone;

        // Finally add hourly array
        if (options.Hourly.Count >= 0)
        {
            bool firstHourlyElement = true;
            uri.Query += "&hourly=";

            foreach (var option in options.Hourly)
            {
                if (firstHourlyElement)
                {
                    uri.Query += option.ToString();
                    firstHourlyElement = false;
                }
                else
                {
                    uri.Query += "," + option.ToString();
                }
            }
        }

        return uri.ToString();
    }

    #region helpers

    private static Dictionary<DateTime, AirQualityForecastParser> GroupByDate(AirQuality forecast)
    {
        var dailyData = new Dictionary<DateTime, AirQualityForecastParser>();
        for (var i = 0; i < forecast.Hourly.Time.Count(); i++)
        {
            var time = TimeUtil.ISO8601ToDateTime(forecast.Hourly.Time[i]);
            var date = time.Date;

            if (!dailyData.ContainsKey(date))
                dailyData[date] = new();

            dailyData[date].Hourly.Add(new HourlyAirQualityParser
            {
                Time = time,
                AQI = forecast.Hourly.Us_aqi[i],
                UV = forecast.Hourly.Uv_index[i]
            });
        }
        return dailyData;
    }

    private static Dictionary<DateTime, WeatherForecastParser> GroupByDate(WeatherForecast forecast)
    {
        var dailyData = new Dictionary<DateTime, WeatherForecastParser>();
        for (int i = 0; i < forecast.Hourly.Time.Count(); i++)
        {
            var time = TimeUtil.ISO8601ToDateTime(forecast.Hourly.Time[i]);
            var date = time.Date;

            if (!dailyData.ContainsKey(date))
                dailyData[date] = new();

            dailyData[date].Hourly.Add(new HourlyWeatherParser
            {
                Time = time,
                WeatherCode = forecast.Hourly.Weathercode[i],
                Visibility = forecast.Hourly.Visibility[i],
                Temperature = forecast.Hourly.Temperature_2m[i],
                ApparentTemperature = forecast.Hourly.Apparent_temperature[i],
                Humidity = forecast.Hourly.Relativehumidity_2m[i],
                DewPoint = forecast.Hourly.Dewpoint_2m[i],
                Pressure = forecast.Hourly.Pressure_msl[i],
                WindSpeed = forecast.Hourly.Windspeed_10m[i],
            });
        }

        foreach (var item in dailyData)
        {
            var date = item.Key;
            var index = Array.FindIndex(forecast.Daily.Time, x => TimeUtil.ISO8601ToDateTime(x).Date == date);
            if (index >= 0)
            {
                dailyData[date].Daily = new DailyWeatherParser()
                {
                    WeatherCode = (int)forecast.Daily.Weathercode[index],
                    TemperatureMin = forecast.Daily.Temperature_2m_min[index],
                    TemperatureMax = forecast.Daily.Temperature_2m_max[index],
                    ApparentTemperatureMin = forecast.Daily.Apparent_temperature_min[index],
                    ApparentTemperatureMax = forecast.Daily.Apparent_temperature_max[index],
                    WindDirection = forecast.Daily.Winddirection_10m_dominant[index],
                    WindSpeed = forecast.Daily.Windspeed_10m_max[index],
                    GustSpeed = forecast.Daily.Windgusts_10m_max[index],
                    Sunrise = TimeUtil.ISO8601ToDateTime(forecast.Daily.Sunrise[index]),
                    Sunset = TimeUtil.ISO8601ToDateTime(forecast.Daily.Sunset[index]),
                };
            }
        }
        return dailyData;
    }

    private sealed class AirQualityForecastParser
    {
        public List<HourlyAirQualityParser> Hourly = [];
    }

    private sealed class WeatherForecastParser
    {
        public List<HourlyWeatherParser> Hourly = [];
        public DailyWeatherParser Daily;
    }

    private sealed class HourlyAirQualityParser
    {
        public DateTime Time { get; set; }
        public float? AQI { get; set; }
        public float? UV { get; set; }
    }

    private sealed class HourlyWeatherParser
    {
        public DateTime Time { get; set; }
        public int? WeatherCode { get; set; }
        public float? Temperature { get; set; }
        public float? ApparentTemperature { get; set; }
        public float? Visibility { get; set; }
        public float? Humidity { get; set; }
        public float? DewPoint { get; set; }
        public float? Pressure { get; set; }
        public float? WindSpeed { get; set; }
    }

    private sealed class DailyWeatherParser
    {
        public int? WeatherCode { get; set; }
        public float? TemperatureMin { get; set; }
        public float? TemperatureMax { get; set; }
        public float? ApparentTemperatureMin { get; set; }
        public float? ApparentTemperatureMax { get; set; }
        public float? WindDirection { get; set; }
        public float? WindSpeed { get; set; }
        public float? GustSpeed { get; set; }
        public DateTime? Sunrise { get; set; }
        public DateTime? Sunset { get; set; }
    }

    #endregion //helpers
}