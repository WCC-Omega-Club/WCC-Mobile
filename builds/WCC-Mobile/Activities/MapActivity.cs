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
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android;
using Android.Support.V4.View;
using WCCMobile.Resources;
using Android.Support.V4.App;
using Android.Support.V7.App;

namespace WCCMobile
{


    [Activity(Label = "Google Map", MainLauncher = false, ParentActivity = typeof(WCCMobile.MainActivity))]
    public class GoogleMapActivity : AppCompatActivity, IOnMapReadyCallback
    {

        static ActivityAttribute attr = null;
        static ActivityAttribute ATTR
        {
            get { return attr ?? new ActivityAttribute(); }
        }


        private GoogleMap map;
        private MapView mapView;
        private MapFragment mapfrag;
        const double latitude = 41.9543;
        const double longitude = -73.5924;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            //mapView = new MapView(this.);
            UpdateCameraPosition();
            MarkOnMap("Location", new LatLng(latitude, longitude), Resource.Drawable.ic_transit);
            if(mapView != null)
                SetContentView(mapView);
            ActionBar.SetIcon(ImageAdapter.Label);
            ActionBar.SetIcon(Android.Resource.Color.Transparent);
            ATTR.MainLauncher = true;
        }

        bool SetUpGoogleMap()
        {
            if (null != map)
                return false;

            mapfrag = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.Map);

          /*  var mapReadyCallback = new OnMapReadyClass();

            mapReadyCallback.MapReadyAction += delegate (GoogleMap googleMap)
            {
                map = googleMap;
                if (map != null)
                {
                    map.MapType = GoogleMap.MapTypeNormal;
                    mapfrag.GetMapAsync(mapReadyCallback);
                    
                }
            };*/
           
            return true;
        }

        void UpdateCameraPosition()
        {
            if (map != null)
            {
                //To initialize the map  
                CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
                builder.Target(new LatLng(latitude, longitude)); //Target to some location hardcoded
                builder.Zoom(15); //Zoom multiplier
                builder.Bearing(45);//bearing is the compass measurement clockwise from North
                builder.Tilt(40); //tilt is the viewing angle from vertical
                CameraPosition cameraPosition = builder.Build();
                CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
                map.AnimateCamera(cameraUpdate);
            }
        }
        void MarkOnMap(string title, LatLng pos, int resourceId)
        {
            try
            {
                var marker = new MarkerOptions();
                marker.SetTitle(title);
                marker.SetPosition(pos); //Resource.Drawable.BlueDot
                marker.SetIcon(BitmapDescriptorFactory.FromResource(resourceId));
                map.AddMarker(marker);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            map = googleMap;
            mapView.GetMapAsync(this);

        }
    }

    //OnMapReadyClass
    public class OnMapReadyClass : Java.Lang.Object, IOnMapReadyCallback
    {
        public GoogleMap Map { get; private set; }
        public event Action<GoogleMap> MapReadyAction;

        public void OnMapReady(GoogleMap googleMap)
        {
            Map = googleMap;

            if (MapReadyAction != null)
                MapReadyAction(Map);
        }
    }
}


/*[Activity(Label = "MapViewActivity")]
    public class MapViewActivity : Activity, IOnMapReadyCallback
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MapView);
            
            InitViews();
        }

        void InitViews()
        {
            MapFragment mapFrag = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.fEventMap);
            mapFrag.GetMapAsync(this);

        }

        public void OnMapReady(GoogleMap googleMap)
        {
            if (googleMap != null)
            {
                // The GoogleMap object is ready to go.
            }
        }
    }
   /* public class MapActivity
    {


        //MyActivityClass.cs 
        GoogleMap map;
        bool SetUpGoogleMap()
        {
            if (null != map) return false;

            MapFragment frag = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.fEventMap);
            var mapReadyCallback = new OnMapReadyClass();

            mapReadyCallback.MapReadyAction += delegate (GoogleMap googleMap)
            {
                map = googleMap;
            };

            frag.GetMapAsync(mapReadyCallback);
            return true;
        }
    }*/



    /// <summary>
    /// Callback object which is constructed once the google map ready event is raised.
    /// </summary>
    /// <seealso cref="Java.Lang.Object" />
    /// <seealso cref="Android.Gms.Maps.IOnMapReadyCallback" />
    public class OnMapReadyClass : Java.Lang.Object, IOnMapReadyCallback
    {
        public GoogleMap Map { get; private set; }
        public event Action<GoogleMap> MapReadyAction;

        public void OnMapReady(GoogleMap googleMap)
        {
            Map = googleMap;

            if (MapReadyAction != null)
                MapReadyAction(Map);
        }
    }
