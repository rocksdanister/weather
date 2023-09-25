using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather.OpenMeteo;

/// <summary>
/// Weather models Variables (https://open-meteo.com/en/docs)
/// </summary>
public class WeatherModelOptions : IEnumerable<WeatherModelOptionsParameter>, ICollection<WeatherModelOptionsParameter>
{
    /// <summary>
    /// Applying every parameter. Using WeatherModelOptions.All will consume around 50mb RAM when performing API requests.
    /// </summary>
    public static WeatherModelOptions All { get { return new WeatherModelOptions((WeatherModelOptionsParameter[])Enum.GetValues(typeof(WeatherModelOptionsParameter))); } }

    /// <summary>
    /// A copy of the current applied parameter. This is a COPY. Editing anything inside this copy won't be applied 
    /// </summary>
    public List<WeatherModelOptionsParameter> Parameter { get { return new List<WeatherModelOptionsParameter>(_parameter); } }

    public int Count => _parameter.Count;

    public bool IsReadOnly => false;

    private readonly List<WeatherModelOptionsParameter> _parameter;

    public WeatherModelOptions(WeatherModelOptionsParameter parameter)
    {
        _parameter = new List<WeatherModelOptionsParameter>();
        Add(parameter);
    }

    public WeatherModelOptions(WeatherModelOptionsParameter[] parameter)
    {
        _parameter = new List<WeatherModelOptionsParameter>();
        Add(parameter);
    }

    public WeatherModelOptions()
    {
        _parameter = new List<WeatherModelOptionsParameter>();
    }

    public WeatherModelOptionsParameter this[int index]
    {
        get { return _parameter[index]; }
        set
        {
            _parameter[index] = value;
        }
    }

    public void Add(WeatherModelOptionsParameter param)
    {
        // Check that the parameter isn't already added
        if (_parameter.Contains(param)) return;

        _parameter.Add(param);
    }

    public void Add(WeatherModelOptionsParameter[] parameters)
    {
        foreach (WeatherModelOptionsParameter paramToAdd in parameters)
        {
            Add(paramToAdd);
        }
    }
    public IEnumerator<WeatherModelOptionsParameter> GetEnumerator()
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
    public bool Contains(WeatherModelOptionsParameter item)
    {
        return _parameter.Contains(item);
    }

    public void CopyTo(WeatherModelOptionsParameter[] array, int arrayIndex)
    {
        _parameter.CopyTo(array, arrayIndex);
    }

    public bool Remove(WeatherModelOptionsParameter item)
    {
        return _parameter.Remove(item);
    }
}

public enum WeatherModelOptionsParameter
{
    best_match,
    ecmwf_ifs04,
    metno_nordic,
    gfs_seamless,
    gfs_global,
    gfs_hrrr,
    jma_seamless,
    jma_msm,
    jma_gsm,
    icon_seamless,
    icon_global,
    icon_eu,
    icon_d2,
    gem_seamless,
    gem_global,
    gem_regional,
    gem_hrdps_continental,
    meteofrance_seamless,
    meteofrance_arpege_world,
    meteofrance_arpege_europe,
    meteofrance_arome_france,
    meteofrance_arome_france_hd
}
