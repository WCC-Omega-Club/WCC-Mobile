using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Android.Gms.Maps;

namespace WCCMobile
{
    /// <summary>
    /// This class is not currently to be used. It should be removed as we 
    /// are using MapView as the map control not MapFragment.
    /// </summary>
    /// <seealso cref="Java.Lang.Object" />
    /// <seealso cref="Android.Gms.Maps.IOnMapReadyCallback" />
    /// <seealso cref="System.IDisposable" />
    public class GetMapHelper : Java.Lang.Object, IOnMapReadyCallback, IDisposable
    {
        
        TaskCompletionSource<GoogleMap> tcs;
        /// <summary>
        /// Initializes a new instance of the <see cref="GetMapHelper"/> class.
        /// </summary>
        public GetMapHelper()
        {
        }

        public async Task<GoogleMap> GetMap(SupportMapFragment frag)
        {
            //check to see if the task is running
            if (tcs != null && tcs.Task.Status == TaskStatus.Running)
            {
                return await tcs.Task;
            }

            //instantiate the task
            tcs = new TaskCompletionSource<GoogleMap>();

            //get the GoogleMap object
            frag.GetMapAsync(this);
            await tcs.Task;

            return tcs.Task.Result;
        }

        void IOnMapReadyCallback.OnMapReady(GoogleMap googleMap)
        {
            tcs.TrySetResult(googleMap);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
