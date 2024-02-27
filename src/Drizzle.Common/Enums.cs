﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Common
{
    public enum ShaderTypes
    {
        clouds,
        rain,
        snow,
        depth,
        fog,
    }

    public enum UserWeatherUnits
    {
        metric,
        imperial,
        hybrid,
        custom
    }

    public enum TemperatureUnits
    {
        degree,
        fahrenheit
    }

    public enum WindSpeedUnits
    {
        kmh,
        mph,
        ms
    }

    public enum VisibilityUnits
    {
        km,
        mi,
    }

    public enum PressureUnits
    {
        hPa_mb
    }

    public enum AppPerformance
    {
        potato,
        performance,
        quality,
        dynamic
    }

    public enum AppTheme
    {
        auto,
        dark,
        light
    }

    public enum WeatherProviders
    {
        OpenMeteo,
        OpenWeatherMap
    }

    //Ref:
    // https://www.nodc.noaa.gov/archive/arc0021/0002199/1.1/data/0-data/HTML/WMO-CODE/WMO4677.HTM
    // https://open-meteo.com/en/docs
    public enum WmoWeatherCode
    {
        ClearSky = 0,
        MainlyClear = 1,
        PartlyCloudy = 2,
        Overcast = 3,
        Haze = 5,
        Dust = 6,
        Mist = 10,
        Fog = 45,
        DepositingRimeFog = 48,
        LightDrizzle = 51,
        ModerateDrizzle = 53,
        DenseDrizzle = 55,
        LightFreezingDrizzle = 56,
        DenseFreezingDrizzle = 57,
        SlightRain = 61,
        ModerateRain = 63,
        HeavyRain = 65,
        LightFreezingRain = 66,
        HeavyFreezingRain = 67,
        SlightSnowFall = 71,
        ModerateSnowFall = 73,
        HeavySnowFall = 75,
        SnowGrains = 77,
        SlightRainShowers = 80,
        ModerateRainShowers = 81,
        ViolentRainShowers = 82,
        SlightSnowShowers = 85,
        HeavySnowShowers = 86,
        Thunderstorm = 95,
        ThunderstormLightHail = 96,
        ThunderstormHeavyHail = 99,
    }
}
