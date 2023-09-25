using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather.OpenMeteo;

public class GeocodingOptions
{
    /// <summary>
    /// Location to search for.
    /// </summary>
    /// <value></value>
    public string Name { get; }

    /// <summary>
    /// Return translated results, if available, otherwise return english or the native location name. 
    /// Lower-cased.
    /// Default is: "en"
    /// </summary>
    /// <value></value>
    public string Language { get; }

    /// <summary>
    /// Default is "json".
    /// </summary>
    /// <value></value>
    public string Format { get; }

    /// <summary>
    /// The number of search results to return.
    /// Default is 10. Up to 100 can be retrieved.
    /// </summary>
    /// <value></value>
    public int Count { get; }

    public GeocodingOptions(string city, string language, int count)
    {
        Name = city;
        Language = language;
        Format = "json";
        Count = count;
    }

    public GeocodingOptions(string city, int count = 10)
    {
        Name = city;
        Language = "en";
        Format = "json";
        Count = count;
    }
}
