using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.AQI
{
    public class Breakpoints
    {
        [JsonPropertyName("code")]
        public Particle Code { get; set; }

        [JsonPropertyName("unit")]
        public ParticleUnit Unit { get; set; }

        [JsonPropertyName("period")]
        public double Period { get; set; }

        [JsonPropertyName("concentrations")]
        public List<Concentration> Concentrations { get; set; }
    }
}
