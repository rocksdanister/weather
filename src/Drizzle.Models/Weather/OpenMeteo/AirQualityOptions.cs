using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.OpenMeteo;

public class AirQualityOptions
{
    /// <summary>
    /// Geographical WGS84 coordinate of the location
    /// </summary>
    public float Latitude { get; set; }

    /// <summary>
    /// Geographical WGS84 coordinate of the location
    /// </summary>
    public float Longitude { get; set; }

    /// <summary>
    /// A list of air quality variables which should be returned.
    /// </summary>
    public HourlyOptions Hourly { get { return _hourly; } set { if (value != null) _hourly = value; } }

    /// <summary>
    /// Default value: "auto". Other values: "cams_europe" for europe or "cams_global" for global domain
    /// </summary>
    public string Domains { get; set; }

    /// <summary>
    /// Default value: "iso8601". Other options: "unixtime"
    /// </summary>
    public string Timeformat { get; set; }

    /// <summary>
    /// Default value: "GMT".
    /// </summary>
    public string Timezone { get; set; }

    /// <summary>
    /// Past days data which should also be returned
    /// </summary>
    public int Past_Days { get; set; }

    /// <summary>
    /// A day must be specified as an ISO8601
    /// </summary>
    public string Start_date { get; set; }

    /// <summary>
    /// A day must be specified as an ISO8601
    /// </summary>
    public string End_date { get; set; }

    private HourlyOptions _hourly = new HourlyOptions();

    public AirQualityOptions()
    {
        Latitude = 0;
        Longitude = 0;
        Hourly = new HourlyOptions();
        Domains = "auto";
        Timeformat = "iso8601";
        Timezone = "GMT";
        Past_Days = 0;
        Start_date = "";
        End_date = "";
    }

    public AirQualityOptions(float latitude, float longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        Hourly = new HourlyOptions();
        Domains = "auto";
        Timeformat = "iso8601";
        Timezone = "GMT";
        Past_Days = 0;
        Start_date = "";
        End_date = "";
    }

    public AirQualityOptions(float latitude, float longitude, HourlyOptions hourly, string domains, string timeformat, string timezone, int past_Days, string start_date, string end_date)
    {
        Latitude = latitude;
        Longitude = longitude;
        if (hourly != null)
            Hourly = hourly;
        Domains = domains;
        Timeformat = timeformat;
        Timezone = timezone;
        Past_Days = past_Days;
        Start_date = start_date;
        End_date = end_date;
    }

    public class HourlyOptions : IEnumerable<HourlyOptionsParameter>, ICollection<HourlyOptionsParameter>
    {
        public static HourlyOptions All { get { return new HourlyOptions((HourlyOptionsParameter[])Enum.GetValues(typeof(HourlyOptionsParameter))); } }

        public List<HourlyOptionsParameter> Parameter { get { return new List<HourlyOptionsParameter>(_parameter); } }

        public int Count => _parameter.Count;

        public bool IsReadOnly => false;

        private readonly List<HourlyOptionsParameter> _parameter = new List<HourlyOptionsParameter>();

        public HourlyOptions(HourlyOptionsParameter parameter)
        {
            Add(parameter);
        }

        public HourlyOptions(HourlyOptionsParameter[] parameter)
        {
            Add(parameter);
        }

        public HourlyOptions()
        {

        }

        public HourlyOptionsParameter this[int index]
        {
            get { return _parameter[index]; }
            set
            {
                _parameter[index] = value;
            }
        }

        public void Add(HourlyOptionsParameter param)
        {
            if (_parameter.Contains(param)) return;

            _parameter.Add(param);
        }

        public void Add(HourlyOptionsParameter[] param)
        {
            foreach (HourlyOptionsParameter paramToAdd in param)
            {
                Add(paramToAdd);
            }
        }

        public IEnumerator<HourlyOptionsParameter> GetEnumerator()
        {
            return _parameter.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            _parameter.Clear();
        }

        public bool Contains(HourlyOptionsParameter item)
        {
            return _parameter.Contains(item);
        }

        public void CopyTo(HourlyOptionsParameter[] array, int arrayIndex)
        {
            _parameter.CopyTo(array, arrayIndex);
        }

        public bool Remove(HourlyOptionsParameter item)
        {
            return _parameter.Remove(item);
        }
    }

    public enum HourlyOptionsParameter
    {
        pm10,
        pm2_5,
        carbon_monoxide,
        nitrogen_dioxide,
        sulphur_dioxide,
        ozone,
        aerosol_optical_depth,
        dust,
        uv_index,
        uv_index_clear_sky,
        alder_pollen,
        birch_pollen,
        grass_pollen,
        mugwort_pollen,
        olive_pollen,
        ragweed_pollen,
        european_aqi,
        european_aqi_pm2_5,
        european_aqi_pm10,
        european_aqi_no2,
        european_aqi_o3,
        european_aqi_so2,
        us_aqi,
        us_aqi_pm2_5,
        us_aqi_pm10,
        us_aqi_no2,
        us_aqi_o3,
        us_aqi_so2,
        us_aqi_co
    }
}