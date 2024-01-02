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
        var dailyWeather = new List<DailyWeather>();
        var dailyVisibility = GetDailyValue(response.Hourly.Visibility.Select(x => x is null ? 0 : (float)x), response.Timezone);
        var dailyHumidity = GetDailyValue(response.Hourly.Relativehumidity_2m.Select(x => x is null ? 0 : (float)x), response.Timezone);
        var dailyDewPoint = GetDailyValue(response.Hourly.Dewpoint_2m.Select(x => x is null ? 0 : (float)x), response.Timezone);
        var dailyPressure = GetDailyValue(response.Hourly.Pressure_msl.Select(x => x is null ? 0 : (float)x), response.Timezone);
        var dailyTemperature = GetDailyValue(response.Hourly.Temperature_2m.Select(x => x is null ? 0 : (float)x), response.Timezone);
        var dailyApparentTemperature = GetDailyValue(response.Hourly.Apparent_temperature.Select(x => x is null ? 0 : (float)x), response.Timezone);
        var dailyWeatherCode = GetDailyValue(response.Hourly.Weathercode.Select(x => x is null ? 0 : (float)x), response.Timezone);
        for (int i = 0; i < 7; i++)
        {
            var weather = new DailyWeather
            {
                // Hourly weather for today, otherwise severe weather for the day
                WeatherCode = i == 0 ? (int)dailyWeatherCode[i] : (int)response.Daily.Weathercode[i],
                Date = ISO8601ToDateTime(response.Daily.Time[i]),
                Sunrise = ISO8601ToDateTime(response.Daily.Sunrise[i]),
                Sunset = ISO8601ToDateTime(response.Daily.Sunset[i]),
                TemperatureMin = response.Daily.Temperature_2m_min[i],
                TemperatureMax = response.Daily.Temperature_2m_max[i],
                ApparentTemperatureMin = response.Daily.Apparent_temperature_min[i],
                ApparentTemperatureMax = response.Daily.Apparent_temperature_max[i],
                WindSpeed = response.Daily.Windspeed_10m_max[i],
                GustSpeed = response.Daily.Windgusts_10m_max[i],
                Temperature = dailyTemperature[i],
                ApparentTemperature = dailyApparentTemperature[i],
                Visibility = dailyVisibility[i]/1000f, //meter -> km
                Humidity = (int)dailyHumidity[i],
                DewPoint = dailyDewPoint[i],
                Pressure = dailyPressure[i],
                WindDirection = response.Daily.Winddirection_10m_dominant[i],
                HourlyWeatherCode = response.Hourly.Weathercode.Select(x => x is null ? 0 : (int)x).Skip(i * 24).Take(24).ToArray(),
                HourlyTemperature = response.Hourly.Temperature_2m.Select(x => x is null ? 0 : (float)x).Skip(i * 24).Take(24).ToArray(),
                HourlyVisibility = response.Hourly.Visibility.Select(x => x is null ? 0 : (float)x/1000).Skip(i * 24).Take(24).ToArray(),
                HourlyHumidity = response.Hourly.Relativehumidity_2m.Select(x => x is null ? 0 : (float)x).Skip(i * 24).Take(24).ToArray(),
                HourlyPressure = response.Hourly.Pressure_msl.Select(x => x is null ? 0 : (float)x).Skip(i * 24).Take(24).ToArray(),
                HourlyWindSpeed = response.Hourly.Windspeed_10m.Select(x => x is null ? 0 : (float)x).Skip(i * 24).Take(24).ToArray(),
                //HourlyTime = response.Hourly.Time.Select(x => IsoToDateTime(x)).ToArray(),
            };
            dailyWeather.Add(weather);
        }
        result.FetchTime = cacheService.LastAccessTime;
        result.Daily = dailyWeather;
        return result;
    }

    public async Task<ForecastAirQuality> QueryAirQualityAsync(float latitude, float longitude)
    {
        var options = new AirQualityOptions
        {
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
        var dailyAirQuality = new List<DailyAirQuality>();
        var dailyAQI = GetDailyValue(response.Hourly.Us_aqi.Select(x => x is null ? 0 : (float)x), response.Timezone, 5);
        var dailyUV = GetDailyValue(response.Hourly.Uv_index.Select(x => x is null ? 0 : (float)x), response.Timezone, 5);
        for (int i = 0; i < 5; i++)
        {
            var airQuality = new DailyAirQuality
            {
                UV = dailyUV[i],
                AQI = (int)dailyAQI[i],
                HourlyAQI = response.Hourly.Us_aqi.Select(x => x is null ? 0 : (float)x).Skip(i * 24).Take(24).ToArray(),
                HourlyUV = response.Hourly.Uv_index.Select(x => x is null ? 0 : (float)x).Skip(i * 24).Take(24).ToArray(),
                Date = ISO8601ToDateTime(response.Hourly.Time[i * 24]),
            };
            dailyAirQuality.Add(airQuality);
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

    private static IReadOnlyList<float> GetDailyValue(IEnumerable<float> values, string timezone, int days = 7)
    {
        var result = new List<float>();
        var currentHour = TimeUtil.GetLocalTime(timezone)?.Hour ?? DateTime.Now.Hour;
        for (int i = 0; i < days; i++)
        {
            // 0 - 23, 24 - 47, 48 - 72..
            var items = values.Skip(i * 24).Take(24);
            result.Add(items.ElementAt(currentHour));
        }
        return result;
    }

    //private static IReadOnlyList<float> GetMidrangeDaily(IEnumerable<float> values, int days = 7)
    //{
    //    var result = new List<float>();
    //    for (int i = 0; i < days; i++)
    //    {
    //        // 0 - 23, 24 - 47, 48 - 72..
    //        var items = values.Skip(i * 24).Take(24);
    //        var midrange = Midrange(items);
    //        result.Add(midrange);
    //    }
    //    return result;
    //}

    //private static IReadOnlyList<float> GetAverageDaily(IEnumerable<float> values, int days = 7)
    //{
    //    var result = new List<float>();
    //    for (int i = 0; i < days; i++)
    //    {
    //        // 0 - 23, 24 - 47, 48 - 72..
    //        var items = values.Skip(i * 24).Take(24);
    //        if (items is not null)
    //        {
    //            var average = Average(items);
    //            result.Add(average);
    //        }
    //    }
    //    return result;
    //}

    //private static float Midrange(float min, float max) => (min + max) / 2f;

    //private static float Midrange(IEnumerable<float> values) => (values.Min() + values.Max()) / 2f;

    //private static float Average(IEnumerable<float> values) => values.Sum() / values.Count();

    private static DateTime ISO8601ToDateTime(string iso8601String) =>
        DateTime.Parse(iso8601String, CultureInfo.InvariantCulture);

    #endregion //helpers
}