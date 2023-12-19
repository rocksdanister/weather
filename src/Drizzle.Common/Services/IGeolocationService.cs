using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.Common.Services
{
    public interface IGeolocationService
    {
        Task<GeoLocation> GetLocationAsync();

        //event EventHandler<GeoLocationResponse> LocationChanged;
    }

    public class GeoLocation()
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
