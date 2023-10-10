using CommunityToolkit.Mvvm.ComponentModel;
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
        private float feelsLike;

        [ObservableProperty]
        private float temperature;

        [ObservableProperty]
        private float temperatureMin;

        [ObservableProperty]
        private float temperatureMax;

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
        private float windSpeed;

        [ObservableProperty]
        private float gustSpeed;

        [ObservableProperty]
        private float windDirection;

        [ObservableProperty]
        private int humidity;

        [ObservableProperty]
        private float visibility;

        [ObservableProperty]
        private float pressure;

        [ObservableProperty]
        private float dewPoint;

        [ObservableProperty]
        private DateTime date;

        [ObservableProperty]
        private DateTime sunrise;

        [ObservableProperty]
        private DateTime sunset;

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
        private string visibilityUnit;

        [ObservableProperty]
        private string pressureUnit;
    }
}
