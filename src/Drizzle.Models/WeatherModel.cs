using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.UserControls;
using Drizzle.Models.Weather;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models
{
    /// <summary>
    /// Daily weather data
    /// </summary>
    public partial class WeatherModel : ObservableObject
    {
        [ObservableProperty]
        private int weatherCode;

        [ObservableProperty]
        private string locationName = "---";

        [ObservableProperty]
        private float? feelsLike;

        [ObservableProperty]
        private float? temperature;

        [ObservableProperty]
        private float? temperatureMin;

        [ObservableProperty]
        private float? temperatureMax;

        [ObservableProperty]
        private int[] hourlyWeatherCode;

        [ObservableProperty]
        private float[] hourlyTemp;

        [ObservableProperty]
        private float[] hourlyVisibility;

        [ObservableProperty]
        private float[] hourlyPressure;

        [ObservableProperty]
        private float[] hourlyHumidity;

        [ObservableProperty]
        private float[] hourlyAirQualityIndex;

        [ObservableProperty]
        private float[] hourlyUltravioletIndex;

        [ObservableProperty]
        private float[] hourlyWindSpeed;

        [ObservableProperty]
        private float[] hourlyApparentTemperature;

        [ObservableProperty]
        private float[] hourlyCloudCover;

        [ObservableProperty]
        private float[] hourlyPrecipitation;

        [ObservableProperty]
        private float? windSpeed;

        [ObservableProperty]
        private float? gustSpeed;

        [ObservableProperty]
        private float? windDirection;

        [ObservableProperty]
        private float? humidity;

        [ObservableProperty]
        private float? cloudCover;

        [ObservableProperty]
        private float? precipitation;

        [ObservableProperty]
        private float? visibility;

        [ObservableProperty]
        private float? pressure;

        [ObservableProperty]
        private float? dewPoint;

        [ObservableProperty]
        private DateTime forecastStartTime;

        [ObservableProperty]
        private DateTime forecastAQStartTime;

        [ObservableProperty]
        private DateTime? sunrise;

        [ObservableProperty]
        private DateTime? sunset;

        [ObservableProperty]
        private int? airQualityIndex = null;

        [ObservableProperty]
        private int? ultravioletIndex = null;

        // Units
        [ObservableProperty]
        private string temperatureUnit;

        [ObservableProperty]
        private string windSpeedUnit;

        [ObservableProperty]
        private VisibilityUnits visibilityUnit;

        [ObservableProperty]
        private string pressureUnit;

        [ObservableProperty]
        private PrecipitationUnits precipitationUnit;

        [ObservableProperty]
        private bool isDaytime;

        [ObservableProperty]
        private GraphModel dayGraph;
    }
}
