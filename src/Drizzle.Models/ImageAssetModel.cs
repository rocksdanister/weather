using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models
{
    public class ImageAssetModel
    {
        public int MetaDataVersion { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        /// <summary>
        /// Depthmap file path if exists
        /// </summary>
        public string DepthPath { get; set; }
        // TimeOnly is better fit but not available .NET standard 2.0
        // ref: https://learn.microsoft.com/en-us/dotnet/standard/datetime/choosing-between-datetime
        public DateTime[] Time { get; set; }
        /// <summary>
        /// Average/prominant background color
        /// </summary>
        public string ColorHex { get; set; }
        /// <summary>
        /// WMO Weather code
        /// </summary>
        public int[] WeatherCode { get; set; }
        public string Attribution { get; set; }
    }
}
