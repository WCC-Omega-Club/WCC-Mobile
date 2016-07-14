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
using Android.Support.V4.App;
using Android.Gms.Maps;
using System.Threading.Tasks;

namespace WCCMobile
{

    


    [Activity(Label = "TestActivity", MainLauncher =false)]
    public class TestActivity : FragmentActivity
    {
        GoogleMap _map;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            if (CheckGooglePlayServices.CheckPlayServices(this) == false)
                this.Finish();

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.myButton);



            button.Click += async delegate {
                if (bAdded == false)
                {
                    bAdded = true;
                    await AddMap();
                }
                else
                {
                    Toast.MakeText(this, "Map already added", ToastLength.Short).Show();
                }
            };
        }

        bool bAdded = false;
        async Task AddMap()
        {
            var fragment = new SupportMapFragment();

            // Insert the fragment by replacing any existing fragment
           /* FragmentManager.BeginTransaction()
                .Replace(Resource.Id, fragment)
                .Commit();
                */
            //get a reference to the GoogleMap object
            _map = await new GetMapHelper().GetMap(fragment);

            //show the zoom controls 
            _map.UiSettings.ZoomControlsEnabled = true;
        }

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
}