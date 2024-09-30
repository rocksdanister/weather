using Drizzle.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.UI.Avalonia.Services
{
    public class GeolocationService : IGeolocationService
    {
        public Task<GeoLocation> GetLocationAsync()
        {
            throw new NotImplementedException();
        }
    }
}
