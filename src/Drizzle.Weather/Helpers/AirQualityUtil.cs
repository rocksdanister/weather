using Drizzle.Common.Helpers;
using Drizzle.Models;
using Drizzle.Models.AQI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Drizzle.Weather.Helpers
{
    public static class AirQualityUtil
    {
        private static readonly string aqiAssetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "aqi-breakpoint.json");
        private readonly static List<Breakpoints> breakpoints = GetbreakPoints();

        // aqi-breakpoint.json ref:
        // https://github.com/ThangLeQuoc/aqi-calculator#calculation-formula
        public static int? GetAirQuality(Particle particle, double concentration_in_ug_per_m3)
        {
            var element = breakpoints.Find(x => x.Code == particle);
            var concentration = element.Unit switch
            {
                ParticleUnit.µg_per_m3 => concentration_in_ug_per_m3,
                ParticleUnit.ppb => Ug_per_m3ToPpb(particle, concentration_in_ug_per_m3),
                ParticleUnit.ppm => Ug_per_m3ToPpm(particle, concentration_in_ug_per_m3),
                _ => throw new NotImplementedException(),
            };
            var roundedConcentration = (float)Math.Round(concentration, 1);
            var breakPoint = element.Concentrations.Find(x => roundedConcentration >= x.Min && roundedConcentration <= x.Max);
            return breakPoint is null ? null : CalculateAQI(breakPoint.Index.Max, breakPoint.Index.Min, breakPoint.Max, breakPoint.Min, roundedConcentration);
        }

        private static int CalculateAQI(int indexHigh, int indexLow, double concentrationHigh, double concentrationLow, float concentration)
        {
            // I = (I_High - I_Low)/(C_High - C_Low) * (C - C_Low) + I_Low
            var index = (indexHigh - indexLow) / (concentrationHigh - concentrationLow) * (concentration - concentrationLow) + indexLow;
            return (int)Math.Round(index);
        }

        public static double Ug_per_m3ToPpb(Particle particle, double concentration)
        {
            // Concentration (mg/m3) = Concentration(ppm) * mass/24.45
            var molecularWeight = GetMolecularWeight(particle);
            return concentration * (24.45f / molecularWeight);
        }

        public static double Ug_per_m3ToPpm(Particle particle, double concentration)
        {
            return PpbToPpm(Ug_per_m3ToPpb(particle, concentration));
        }

        public static double PpbToPpm(double concentration)
        {
            return concentration * 0.001f;
        }

        public static double PpmToPpb(double concentration)
        {
            return concentration * 1000f;
        }

        private static double GetMolecularWeight(Particle particle)
        {
            return particle switch
            {
                Particle.PM2_5 => throw new InvalidOperationException(),
                Particle.PM10 => throw new InvalidOperationException(),
                Particle.NO2 => 46.01,
                Particle.SO2 => 64.07,
                Particle.CO => 28.01,
                Particle.O3_8h => 48,
                Particle.O3_1h => 48,
                _ => throw new NotImplementedException(),
            };
        }

        private static List<Breakpoints> GetbreakPoints()
        {
            return JsonUtil.Load<List<Breakpoints>>(aqiAssetPath);
        }
    }
}
