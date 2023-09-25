using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather;

public class Location
{
    /// <summary>
    /// Combines Name, Country and Admin1 based on some rules.
    /// </summary>
    public string? DisplayName { get; set; }

    public string? Name { get; set; }

    public string? Country { get; set; }

    public string? Admin1 { get; set; }

    public float Latitude { get; set; }

    public float Longitude { get; set; }
}
