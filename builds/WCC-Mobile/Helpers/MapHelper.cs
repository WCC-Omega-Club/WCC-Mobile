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

    class GetMapHelper : Java.Lang.Object, IOnMapReadyCallback, IDisposable
    {
        TaskCompletionSource<GoogleMap> tcs;

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
