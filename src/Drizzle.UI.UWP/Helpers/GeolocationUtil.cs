using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Core;
using System.Diagnostics;

namespace Drizzle.UI.UWP.Helpers
{
    // Ref: https://learn.microsoft.com/en-us/windows/uwp/maps-and-location/get-location
    public class GeolocationUtil
    {
        public async Task GetLocation()
        {
            throw new NotImplementedException();

            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    Debug.WriteLine("Waiting for location update...");

                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 0 };

                    // Subscribe to the StatusChanged event to get updates of location status changes.
                    //_geolocator.StatusChanged += OnStatusChanged;

                    // Carry out the operation.
                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    Debug.WriteLine("Location updated...");
                    break;

                case GeolocationAccessStatus.Denied:
                    Debug.WriteLine("Access to location is denied.");
                    //_rootPage.NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);
                    //LocationDisabledMessage.Visibility = Visibility.Visible;
                    //UpdateLocationData(null);
                    break;

                case GeolocationAccessStatus.Unspecified:
                    Debug.WriteLine("Location unspecified error.");
                    //_rootPage.NotifyUser("Unspecified error.", NotifyType.ErrorMessage);
                    //UpdateLocationData(null);
                    break;
            }
        }

        //async private void OnStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        //{
        //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {
        //        // Show the location setting message only if status is disabled.
        //        LocationDisabledMessage.Visibility = Visibility.Collapsed;

        //        switch (e.Status)
        //        {
        //            case PositionStatus.Ready:
        //                // Location platform is providing valid data.
        //                ScenarioOutput_Status.Text = "Ready";
        //                _rootPage.NotifyUser("Location platform is ready.", NotifyType.StatusMessage);
        //                break;

        //            case PositionStatus.Initializing:
        //                // Location platform is attempting to acquire a fix.
        //                ScenarioOutput_Status.Text = "Initializing";
        //                _rootPage.NotifyUser("Location platform is attempting to obtain a position.", NotifyType.StatusMessage);
        //                break;

        //            case PositionStatus.NoData:
        //                // Location platform could not obtain location data.
        //                ScenarioOutput_Status.Text = "No data";
        //                _rootPage.NotifyUser("Not able to determine the location.", NotifyType.ErrorMessage);
        //                break;

        //            case PositionStatus.Disabled:
        //                // The permission to access location data is denied by the user or other policies.
        //                ScenarioOutput_Status.Text = "Disabled";
        //                _rootPage.NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);

        //                // Show message to the user to go to location settings.
        //                LocationDisabledMessage.Visibility = Visibility.Visible;

        //                // Clear any cached location data.
        //                UpdateLocationData(null);
        //                break;

        //            case PositionStatus.NotInitialized:
        //                // The location platform is not initialized. This indicates that the application
        //                // has not made a request for location data.
        //                ScenarioOutput_Status.Text = "Not initialized";
        //                _rootPage.NotifyUser("No request for location is made yet.", NotifyType.StatusMessage);
        //                break;

        //            case PositionStatus.NotAvailable:
        //                // The location platform is not available on this version of the OS.
        //                ScenarioOutput_Status.Text = "Not available";
        //                _rootPage.NotifyUser("Location is not available on this version of the OS.", NotifyType.ErrorMessage);
        //                break;

        //            default:
        //                ScenarioOutput_Status.Text = "Unknown";
        //                _rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
        //                break;
        //        }
        //    });
        //}
    }
}
