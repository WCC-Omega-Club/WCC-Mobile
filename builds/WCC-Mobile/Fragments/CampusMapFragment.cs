﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps.Model;
using Android.Gms.Maps;
using Android.Support.V4.View;
using Android.Animation;
using Android.Graphics;
using System.Reflection;
using WCCMobile.Controls;
using WCCMobile.Extensions;
using WCCMobile.Models;
using XamSvg;
using Android.Support.V7.Widget;
using WCCMobile.Adapters;

namespace WCCMobile
{
    public class CampusMapFragment : Android.Support.V4.App.Fragment, ViewTreeObserver.IOnGlobalLayoutListener, ICampusSection, IOnMapReadyCallback, IOnStreetViewPanoramaReadyCallback
    {
        readonly string ObjectName = "CampusMapFragment";
        /// <summary>
        /// The existing markers
        /// </summary>
        Dictionary<int, Marker> existingMarkers = new Dictionary<int, Marker>();
        /// <summary>
        /// The location pin
        /// </summary>
        Marker locationPin;
        /// <summary>
        /// The map fragment
        /// </summary>
        MapView mapFragment;
        /// <summary>
        /// The google map
        /// </summary>
        GoogleMap map;
        /// <summary>
        /// The street view fragment
        /// </summary>
        StreetViewPanoramaView streetViewFragment;
        /// <summary>
        /// The street view panorama
        /// </summary>
        StreetViewPanorama streetPanorama;
        /// <summary>
        /// The data provider
        /// </summary>
        ScheduleObserver dataProvider = ScheduleObserver.Instance;
        ScheduleHistory scheduleHistory = new ScheduleHistory();

        bool loading;
        bool showedStale;
        InfoBarController flashBar;
        IMenuItem searchItem;
        ScheduleManager scheduleManager;
        TextView lastUpdateText;
        PinFactory pinFactory;
        InfoPane pane;
        RecyclerView recyclerView;
        GenericAdapter<Schedule, ScheduleViewHolder> scheduleAdapter;

        SvgBitmapDrawable starOnDrawable;
        SvgBitmapDrawable starOffDrawable;
        

        int currentShownID = -1;
        Marker currentShownMarker;
        CameraPosition oldPosition;
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusMapFragment"/> class.
        /// </summary>
        public CampusMapFragment() : base()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusMapFragment"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CampusMapFragment(Context context) : base()
        {
            HasOptionsMenu = true;
        }
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                return "MapFragment";
            }
        }
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get
            {  return "Campus Map"; }
        }
        /// <summary>
        /// Gets the current shown identifier.
        /// </summary>
        /// <value>
        /// The current shown identifier.
        /// </value>
        internal int CurrentShownId
        {
            get { return currentShownID;  }
        }
        /// <summary>
        /// Refreshes the data.
        /// </summary>
        public void RefreshData()
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name);
            FillUpMap(forceRefresh: false);
        }
        /// <summary>
        /// Called when [activity created].
        /// </summary>
        /// <param name="savedInstanceState">State of the saved instance.</param>
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name);
            base.OnActivityCreated(savedInstanceState);
            var context = Activity;
            this.pinFactory = new PinFactory(context);
            this.scheduleManager = ScheduleManager.Obtain(context);
        }
        /// <summary>
        /// Called when [start].
        /// </summary>
        public override void OnStart()
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name);

            base.OnStart();
            RefreshData();
        }
        /// <summary>
        /// Called when [global layout].
        /// </summary>
        public void OnGlobalLayout()
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name);

            Activity.RunOnUiThread(() => pane.SetState(InfoPane.State.Opened, animated: false));
            View.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
        }
        /// <summary>
        /// Called when [create view].
        /// </summary>
        /// <param name="inflater">The inflater.</param>
        /// <param name="container">The container.</param>
        /// <param name="savedInstanceState">State of the saved instance.</param>
        /// <returns></returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Log.Debug(ObjectName, MethodBase.GetCurrentMethod().Name + $"With args: {inflater} , {container} ");

            var view = inflater.Inflate(Resource.Layout.MapLayout, container, false);
            mapFragment = view.FindViewById<MapView>(Resource.Id.Map);
            mapFragment.OnCreate(savedInstanceState);
            lastUpdateText = view.FindViewById<TextView>(Resource.Id.UpdateTimeText);
            SetupInfoPane(view);
            flashBar = new InfoBarController(view);
            streetViewFragment = pane.FindViewById<StreetViewPanoramaView>(Resource.Id.streetViewPanorama);
            
            //SetUpSlideUpPane(view);
            streetViewFragment.OnCreate(savedInstanceState);
            return view;
        }
        /*public void SetUpSlideUpPane(View view)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $"With args: {view} ");

            slideUpPanel = view.FindViewById<SlidingUpPanelLayout>(Resource.Id.sliding_layout);

            slideUpPanel.ShadowDrawable = Resources.GetDrawable(Resource.Drawable.ic_map_footer_details_down);
            slideUpPanel.AnchorPoint = 0.3f;
            slideUpPanel.PanelExpanded += (s, e) => Log.Info(Tag, "PanelExpanded");
            slideUpPanel.PanelCollapsed += (s, e) => Log.Info(Tag, "PanelCollapsed");
            slideUpPanel.PanelAnchored += (s, e) => Log.Info(Tag, "PanelAnchored");
            slideUpPanel.PanelSlide += (s, e) =>
            {
                if (e.SlideOffset < 0.2)
                {
                    slideUpPanel.ShowContextMenu();
                }
                else
                {
                   // if (!slideUpPanel.IsShowing)
                    //    SupportActionBar.Show();
                }
            };

        }*/
        /// <summary>
        /// Sets up the information pane.
        /// </summary>
        /// <param name="view">The view.</param>
        private void SetupInfoPane(View view)
        {
            Log.Debug(ObjectName, MethodBase.GetCurrentMethod().Name + $" with argument: {view}");
           
            pane = view.FindViewById<InfoPane>(Resource.Id.infoPane);
            List<Schedule> schedule = new List<Schedule>();
            schedule.Add(new Schedule(1, DayOfWeek.Monday, new Course(1, "Data Structures 201", "Computer Science", "Learn how to become a meat space architect", "Technology Building", "101", "Steven Miller"),
                new Times(1, new TimeSpan(9, 20, 0), new TimeSpan(10, 10, 0))));
            schedule.Add(new Schedule(2, DayOfWeek.Monday, new Course(2, "Computer Programing 2", "Computer Science", "Learn how to become a meat space architect", "Technology Building", "101", "Robert Sciabarrasci"),
                new Times(2, new TimeSpan(9, 20, 0), new TimeSpan(10, 10, 0))));
            scheduleAdapter = new GenericAdapter<Schedule, ScheduleViewHolder>(schedule, Resource.Layout.ScheduleCardView, (vw) => new ScheduleViewHolder(vw), this.Context);

            recyclerView = pane.FindViewById<RecyclerView>(Resource.Id.recyclerView);
           
            recyclerView.SetAdapter(scheduleAdapter);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this.Context));
            pane.StateChanged += HandlePaneStateChanged;
            view.ViewTreeObserver.AddOnGlobalLayoutListener(this);
        }
        /// <summary>
        /// Called when [view created].
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="savedInstanceState">State of the saved instance.</param>
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $" with argument: {view}");

            base.OnViewCreated(view, savedInstanceState);

            view.SetBackgroundDrawable(AndroidExtensions.DefaultBackground);

            mapFragment?.GetMapAsync(this);




            // Setup info pane
            SetSvgImage(pane, Resource.Id.bikeImageView, Resource.Raw.min);
            SetSvgImage(pane, Resource.Id.lockImageView, Resource.Raw.ic_lock);
            SetSvgImage(pane, Resource.Id.stationLock, Resource.Raw.station_lock);
            SetSvgImage(pane, Resource.Id.bikeNumberImg, Resource.Raw.bike_number);
            SetSvgImage(pane, Resource.Id.clockImg, Resource.Raw.clock);
            SetSvgImage(pane, Resource.Id.stationNotInstalled, Resource.Raw.not_installed);
            starOnDrawable = SvgFactory.GetDrawable(Resources, Resource.Raw.star_on);
            starOffDrawable = SvgFactory.GetDrawable(Resources, Resource.Raw.star_off);
            var starBtn = pane.FindViewById<ImageButton>(Resource.Id.StarButton);
            starBtn.Click += HandleStarButtonChecked;
            streetViewFragment?.GetStreetViewPanoramaAsync(this);
        }
        /// <summary>
        /// Sets the SVG image.
        /// </summary>
        /// <param name="baseView">The base view.</param>
        /// <param name="viewId">The view identifier.</param>
        /// <param name="resId">The resource identifier.</param>
        private void SetSvgImage(View baseView, int viewId, int resId)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $" with args: {baseView}, ViewId: {viewId}, ResId{resId}");

            var view = baseView.FindViewById<ImageView>(viewId);
            if (view == null)
                return;
            var img = SvgFactory.GetDrawable(Resources, resId);
            view.SetImageDrawable(img);
        }
        /// <summary>
        /// Called when [map ready].
        /// </summary>
        /// <param name="googleMap">The google map.</param>
        public void OnMapReady(GoogleMap googleMap)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $" with argument: {googleMap}");

            this.map = googleMap;
            MapsInitializer.Initialize(Activity.ApplicationContext);

            googleMap.MyLocationEnabled = true;
            googleMap.UiSettings.MyLocationButtonEnabled = false;
            googleMap.UiSettings.IndoorLevelPickerEnabled = true;
            googleMap.TrafficEnabled = true;
            googleMap.BuildingsEnabled = true;
            googleMap.MapClick += HandleMapClick;
            googleMap.MarkerClick += HandleMarkerClick;

            var position = PreviousCameraPosition;
            if (position != null)
            {
                //default map initialization;
                googleMap.MoveCamera(CameraUpdateFactory.NewCameraPosition(position));
            }
        }
        /// <summary>
        /// Called when [street view panorama ready].
        /// </summary>
        /// <param name="panorama">The panorama.</param>
        public void OnStreetViewPanoramaReady(StreetViewPanorama panorama)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $" with argument: {panorama}");

            this.streetPanorama = panorama;
            panorama.UserNavigationEnabled = false;
            panorama.StreetNamesEnabled = false;
            panorama.StreetViewPanoramaClick += HandleMapButtonClick;
        }
        /// <summary>
        /// Handles the pane state changed.
        /// </summary>
        /// <param name="state">The state.</param>
        private void HandlePaneStateChanged(InfoPane.State state)
        {
            Log.Debug(ObjectName, MethodBase.GetCurrentMethod().Name + $" with argument: {state}");

            var time = Resources.GetInteger(Android.Resource.Integer.ConfigShortAnimTime);
            var enabled = state != InfoPane.State.FullyOpened;
            map.UiSettings.ScrollGesturesEnabled = enabled;
            map.UiSettings.ZoomGesturesEnabled = enabled;
            if (state == InfoPane.State.FullyOpened && currentShownMarker != null)
            {
                oldPosition = map.CameraPosition;
                var destX = mapFragment?.Width / 2;
                var destY = (mapFragment?.Height - pane.Height) / 2;
                var currentPoint = map.Projection.ToScreenLocation(currentShownMarker.Position);
                var scroll = CameraUpdateFactory.ScrollBy((float)-destX + (float)currentPoint.X, (float)-destY + (float)currentPoint.Y);
                map.AnimateCamera(scroll, time, null);
            }
            else if (oldPosition != null)
            {
                map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(oldPosition), time, null);
                oldPosition = null;
            }
        }
        /// <summary>
        /// Handles the map button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="StreetViewPanorama.StreetViewPanoramaClickEventArgs"/> instance containing the event data.</param>
        private void HandleMapButtonClick(object sender, StreetViewPanorama.StreetViewPanoramaClickEventArgs e)
        {
            Log.Debug(ObjectName, MethodBase.GetCurrentMethod().Name);

            var items = dataProvider.LastScheduleItems;
            if (items == null || currentShownID == -1)
                return;

            int itemIndex = Array.FindIndex(items, s => s.TimesId == currentShownID);
            if (itemIndex == -1)
                return;
            Schedule item = items[itemIndex];
            string location = Course.GetLocation(item.Course.Building).ToString();
            Android.Net.Uri uri = Android.Net.Uri.Parse(location);
            Intent intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        }
        /// <summary>
        /// Called when [create options menu].
        /// </summary>
        /// <param name="menu">The menu.</param>
        /// <param name="inflater">The inflater.</param>
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            Log.Debug(ObjectName, MethodBase.GetCurrentMethod().Name + $" with argument: {menu}");

            inflater.Inflate(Resource.Menu.map_menu, menu);
            searchItem = menu.FindItem(Resource.Id.menu_search);
            var test = MenuItemCompat.GetActionView(searchItem);
            var searchView = test.JavaCast<Android.Support.V7.Widget.SearchView>();

            SetupSearchInput(searchView);
        }
        /// <summary>
        /// Setups the search input.
        /// </summary>
        /// <param name="searchView">The search view.</param>
        private void SetupSearchInput(Android.Support.V7.Widget.SearchView searchView)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $" with argument: {searchView}");

            var searchManager = Activity.GetSystemService(Context.SearchService).JavaCast<SearchManager>();
            searchView.SetIconifiedByDefault(false);
            var searchInfo = searchManager.GetSearchableInfo(Activity.ComponentName);
            searchView.SetSearchableInfo(searchInfo);
        }
        /// <summary>
        /// Called when [options item selected].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $" with argument: {item}");

            if (item.ItemId == Resource.Id.menu_refresh)
            {
                FillUpMap(forceRefresh: true);
                return true;
            }
            else if (item.ItemId == Resource.Id.menu_mylocation)
            {
                CenterMapOnUser();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
        /// <summary>
        /// Called when [view state restored].
        /// </summary>
        /// <param name="savedInstanceState">State of the saved instance.</param>
        public override void OnViewStateRestored(Bundle savedInstanceState)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $" with argument: {savedInstanceState}");

            base.OnViewStateRestored(savedInstanceState);
            if (savedInstanceState != null && savedInstanceState.ContainsKey("previousPosition"))
            {
                CameraPosition pos = savedInstanceState.GetParcelable("previousPosition") as CameraPosition;
                if (pos != null)
                {
                    CameraUpdate update = CameraUpdateFactory.NewCameraPosition(pos);
                    map.MoveCamera(update);
                }
            }
        }
        /// <summary>
        /// Called when [resume].
        /// </summary>
        public override void OnResume()
        {
            base.OnResume();
            mapFragment?.OnResume();
            streetViewFragment?.OnResume();
        }
        /// <summary>
        /// Called when [low memory].
        /// </summary>
        public override void OnLowMemory()
        {
            base.OnLowMemory();
            mapFragment?.OnLowMemory();
            streetViewFragment?.OnLowMemory();
        }
        /// <summary>
        /// Called when [pause].
        /// </summary>
        public override void OnPause()
        {
            base.OnPause();

            mapFragment?.OnPause();
            PreviousCameraPosition = map.CameraPosition;
            streetViewFragment?.OnPause();
        }
        /// <summary>
        /// Called when [destroy].
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            mapFragment?.OnDestroy();
            streetViewFragment?.OnDestroy();
        }
        /// <summary>
        /// Called when [save instance state].
        /// </summary>
        /// <param name="outState">State of the out.</param>
        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            mapFragment?.OnSaveInstanceState(outState);
            streetViewFragment?.OnSaveInstanceState(outState);
        }
        /// <summary>
        /// Handles the map click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="GoogleMap.MapClickEventArgs"/> instance containing the event data.</param>
        private void HandleMapClick(object sender, GoogleMap.MapClickEventArgs e)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $" with argument: {e}");

            currentShownID = -1;
            currentShownMarker = null;
            pane?.SetState(InfoPane.State.Opened);
        }
        /// <summary>
        /// Handles the marker click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="GoogleMap.MarkerClickEventArgs"/> instance containing the event data.</param>
        private void HandleMarkerClick(object sender, GoogleMap.MarkerClickEventArgs e)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $" with argument: {e}");

            e.Handled = true;
            if (e?.Marker.Title == null)
                return;

            OpenScheduleItemWithMarker(e.Marker);
        }
        /// <summary>
        /// Handles the star button checked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleStarButtonChecked(object sender, EventArgs e)
        {
            if (currentShownID == -1)
                return;
            var starButton = (ImageButton)sender;
        
        }
        /// <summary>
        /// Fills up map.
        /// </summary>
        /// <param name="forceRefresh">if set to <c>true</c> [force refresh].</param>
        public async void FillUpMap(bool forceRefresh)
        {
            if (loading)
                return;
            loading = true;
            if (pane != null && pane.Opened)
                //pane.SetState(InfoPane.State.Closed, animated: false);
                pane.SetState(InfoPane.State.Opened, animated: false);
            flashBar.ShowLoading();

            try
            {
                var events = await dataProvider.GetEvents(forceRefresh);
                if (events.Length == 0)
                {
                    Toast.MakeText(Activity, Resource.String.load_error, ToastLength.Long).Show();
                }
                else
                {
                   // await SetSchedulePins(events);
                }
                lastUpdateText.Text = "Last refreshed: " + DateTime.Now.ToShortTimeString();
            }
            catch (Exception e)
            {

                Log.Debug("DataFetcher", e.ToString());
            }

            flashBar.ShowLoaded();
            showedStale = false;
            loading = false;
        }
        /// <summary>
        /// Sets the schedule pins.
        /// </summary>
        /// <param name="courses">The courses.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns></returns>
        private async Task SetSchedulePins(Schedule[] courses, float alpha = 1)
        {
            var scheduleItemsToUpdate = courses.Where(item =>
            {
                Marker marker;
                var stats = item.Times.StartTime + "-" + item.Times.EndTime;
                if (existingMarkers.TryGetValue(item.TimesId, out marker))
                {
                    if (marker.Snippet == stats && !showedStale)
                        return false;
                    marker.Remove();
                }
                return true;
            }).ToArray();

            var pins = await Task.Run(() => scheduleItemsToUpdate.ToDictionary(item => item.TimesId, item =>
            {
                var w = 24.ToPixels();
                var h = 40.ToPixels();
                return pinFactory.GetClosedPin(w, h);
            }));

            foreach (var scheduleItem in scheduleItemsToUpdate)
            {
                var pin = pins[scheduleItem.TimesId];

                var snippet = scheduleItem.Times.StartTime + "-" + scheduleItem.Times.EndTime;


           MarkerOptions markerOptions = new MarkerOptions()
          .SetTitle(scheduleItem.Course.Major + "|" + scheduleItem.Course.Name)
          .SetSnippet(snippet)
          .SetPosition(Course.GetLatLng(scheduleItem.Course.Building))
          .SetIcon(BitmapDescriptorFactory.FromBitmap(pin));
                existingMarkers[scheduleItem.TimesId] = map.AddMarker(markerOptions);
            }
        }
        /// <summary>
        /// Center and open location related info on map.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="zoom">The zoom.</param>
        /// <param name="animDurationID">The anim duration identifier.</param>
        public void CenterAndOpenOnMap(long id,
                                         float zoom = 13,
                                         int animDurationID = Android.Resource.Integer.ConfigShortAnimTime)
        {
            Marker marker;
            if (!existingMarkers.TryGetValue((int)id, out marker))
                return;
            CenterAndOpenOnMap(marker, zoom, animDurationID);
        }
        /// <summary>
        /// Center amd open on map.
        /// </summary>
        /// <param name="marker">The marker.</param>
        /// <param name="zoom">The zoom.</param>
        /// <param name="animDurationID">The anim duration identifier.</param>
        public void CenterAndOpenOnMap(Marker marker,
                                         float zoom = 13,
                                         int animDurationID = Android.Resource.Integer.ConfigShortAnimTime)
        {
            var latLng = marker.Position;
            var camera = CameraUpdateFactory.NewLatLngZoom(latLng, zoom);
            var time = Resources.GetInteger(animDurationID);
            map.AnimateCamera(camera, time, new MapAnimCallback(() => OpenScheduleItemWithMarker(marker)));
        }
        /// <summary>
        /// Opens the schedule item with marker.
        /// </summary>
        /// <param name="marker">The marker.</param>
        public void OpenScheduleItemWithMarker(Marker marker)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $" with argument: {marker}");

            var id = pane.FindViewById<TextView>(Resource.Id.InfoViewName);
            var name = pane.FindViewById<TextView>(Resource.Id.InfoViewSecondName);
            var major = pane.FindViewById<TextView>(Resource.Id.InfoViewBikeNumber);
            var timeslots = pane.FindViewById<TextView>(Resource.Id.InfoViewSlotNumber);
            var starButton = pane.FindViewById<ImageButton>(Resource.Id.StarButton);

            var splitTitle = marker.Title.Split('|');
            id.Text = splitTitle[1];
            name.Text = splitTitle[2];

            currentShownID = int.Parse(splitTitle[0]);
            currentShownMarker = marker;

            var isLocked = string.IsNullOrEmpty(marker.Snippet);
            var isNotInstalled = marker.Snippet == "not_installed";
            pane.FindViewById(Resource.Id.stationStats).Visibility = (isNotInstalled || isLocked) ? ViewStates.Gone : ViewStates.Visible;
            pane.FindViewById(Resource.Id.stationLock).Visibility = isLocked ? ViewStates.Visible : ViewStates.Gone;
            pane.FindViewById(Resource.Id.stationNotInstalled).Visibility = isNotInstalled ? ViewStates.Visible : ViewStates.Gone;
            if (!isLocked && !isNotInstalled)
            {
                var splitNumbers = marker.Snippet.Split('|');
                major.Text = splitNumbers[0];
                timeslots.Text = splitNumbers[1];
            }

            var scheds = scheduleManager.LastSchedule ?? scheduleManager.GetScheduleItemIDs();
           
            var streetView = streetPanorama;
            streetView.SetPosition(marker.Position);

            LoadScheduleHistory(currentShownID);

            pane.SetState(InfoPane.State.Opened);
        }
        /// <summary>
        /// Loads the event schedule history.
        /// </summary>
        /// <param name="ID">The station identifier.</param>
        async void LoadScheduleHistory(int ID)
        {
            const char DownArrow = '↘';
            const char UpArrow = '↗';

            var historyTimes = new int[]
            {
                Resource.Id.historyTime1,
                Resource.Id.historyTime2,
                Resource.Id.historyTime3,
                Resource.Id.historyTime4,
                Resource.Id.historyTime5
            };
            var historyValues = new int[]
            {
                Resource.Id.historyValue1,
                Resource.Id.historyValue2,
                Resource.Id.historyValue3,
                Resource.Id.historyValue4,
                Resource.Id.historyValue5
            };

            foreach (var ht in historyTimes)
                pane.FindViewById<TextView>(ht).Text = "-:-";
            foreach (var hv in historyValues)
            {
                var v = pane.FindViewById<TextView>(hv);
                v.Text = "-";
                v.SetTextColor(Color.Rgb(0x90, 0x90, 0x90));
            }
        
        }
        /// <summary>
        /// Centers the map on location.
        /// </summary>
        /// <param name="latLng">The lat LNG.</param>
        public void CenterMapOnLocation(LatLng latLng)
        {
            var camera = CameraUpdateFactory.NewLatLngZoom(latLng, 16);
            map.AnimateCamera(camera,
                new MapAnimCallback(() => SetLocationPin(latLng)));
        }
        /// <summary>
        /// Called when [search intent].
        /// </summary>
        /// <param name="intent">The intent.</param>
        public void OnSearchIntent(Intent intent)
        {
            searchItem.CollapseActionView();
            if (intent.Action != Intent.ActionSearch)
                return;
            var serial = (string)intent.Extras.Get(SearchManager.ExtraDataKey);
            if (serial == null)
                return;
            var latlng = serial.Split('|');
            var finalLatLng = new LatLng(double.Parse(latlng[0]),
                         double.Parse(latlng[1]));
            CenterMapOnLocation(finalLatLng);
        }
        /// <summary>
        /// Gets or sets the previous camera position.
        /// </summary>
        /// <value>
        /// The previous camera position.
        /// </value>
        CameraPosition PreviousCameraPosition
        {
            get
            {
                var prefs = Activity.GetPreferences(FileCreationMode.Private);
                if (!prefs.Contains("lastPosition-bearing")
                || !prefs.Contains("lastPosition-tilt")
                || !prefs.Contains("lastPosition-zoom")
                || !prefs.Contains("lastPosition-lat")
                || !prefs.Contains("lastPosition-lon"))
                {

                    return new CameraPosition.Builder()
                        .Zoom(13)
                        .Target(new LatLng(41.068251, -73.789583))
                        .Build();
                }

                var bearing = prefs.GetFloat("lastPosition-bearing", 0);
                var tilt = prefs.GetFloat("lastPosition-tilt", 0);
                var zoom = prefs.GetFloat("lastPosition-zoom", 0);
                var latitude = prefs.GetFloat("lastPosition-lat", 0);
                var longitude = prefs.GetFloat("lastPosition-lon", 0);

                return new CameraPosition.Builder()
                    .Bearing(bearing)
                    .Tilt(tilt)
                    .Zoom(zoom)
                    .Target(new LatLng(latitude, longitude))
                    .Build();
            }
            set
            {
                var position = map.CameraPosition;
                var prefs = Activity.GetPreferences(FileCreationMode.Private);
                using (var editor = prefs.Edit())
                {
                    editor.PutFloat("lastPosition-bearing", position.Bearing);
                    editor.PutFloat("lastPosition-tilt", position.Tilt);
                    editor.PutFloat("lastPosition-zoom", position.Zoom);
                    editor.PutFloat("lastPosition-lat", (float)position.Target.Latitude);
                    editor.PutFloat("lastPosition-lon", (float)position.Target.Longitude);
                    editor.Commit();
                }
            }
        }
       
        /// <summary>
        /// Centers the map on user.
        /// </summary>
        /// <returns></returns>
        private bool CenterMapOnUser()
        {
            var location = map.MyLocation;
            if (location == null)
                return false;
            var userPos = new LatLng(location.Latitude, location.Longitude);
            var camPos = map.CameraPosition.Target;
            var needZoom = TruncateDigit(camPos.Latitude, 4) == TruncateDigit(userPos.Latitude, 4)
                  && TruncateDigit(camPos.Longitude, 4) == TruncateDigit(userPos.Longitude, 4);
            var cameraUpdate = needZoom ?
                CameraUpdateFactory.NewLatLngZoom(userPos, map.CameraPosition.Zoom + 2) :
                    CameraUpdateFactory.NewLatLng(userPos);
            map.AnimateCamera(cameraUpdate);
            return true;
        }
        /// <summary>
        /// Sets the location pin.
        /// </summary>
        /// <param name="finalLatLng">The final lat LNG.</param>
        private void SetLocationPin(LatLng finalLatLng)
        {
            if (locationPin != null)
            {
                locationPin.Remove();
                locationPin = null;
            }
            var proj = map.Projection;
            var location = proj.ToScreenLocation(finalLatLng);
            location.Offset(0, -(35.ToPixels()));
            var startLatLng = proj.FromScreenLocation(location);

            new Handler(Activity.MainLooper).PostDelayed(() =>
            {
                var opts = new MarkerOptions()
      .SetPosition(startLatLng)
                .SetIcon(BitmapDescriptorFactory.DefaultMarker(Resource.Drawable.pin));
                var marker = map.AddMarker(opts);
                var animator = ObjectAnimator.OfObject(marker, "position", new LatLngEvaluator(), startLatLng, finalLatLng);
                animator.SetDuration(1000);
                animator.SetInterpolator(new Android.Views.Animations.BounceInterpolator());
                animator.Start();
                locationPin = marker;
            }, 800);
        }

        class LatLngEvaluator : Java.Lang.Object, ITypeEvaluator
        {
            /// <summary>
            /// To be added.
            /// </summary>
            /// <param name="fraction">To be added.</param>
            /// <param name="startValue">To be added.</param>
            /// <param name="endValue">To be added.</param>
            /// <returns>
            /// To be added.
            /// </returns>
            /// <remarks>
            /// To be added.
            /// </remarks>
            public Java.Lang.Object Evaluate(float fraction, Java.Lang.Object startValue, Java.Lang.Object endValue)
            {
                var start = (LatLng)startValue;
                var end = (LatLng)endValue;

                return new LatLng(start.Latitude + fraction * (end.Latitude - start.Latitude),
                    start.Longitude + fraction * (end.Longitude - start.Longitude));
            }
        }

        class MapAnimCallback : Java.Lang.Object, GoogleMap.ICancelableCallback
        {
            Action callback;
            /// <summary>
            /// Initializes a new instance of the <see cref="MapAnimCallback"/> class.
            /// </summary>
            /// <param name="callback">The callback.</param>
            public MapAnimCallback(Action callback)
            {
                this.callback = callback;
            }
            /// <summary>
            /// Called when [cancel].
            /// </summary>
            public void OnCancel()
            {
            }
            /// <summary>
            /// Called when [finish].
            /// </summary>
            public void OnFinish()
            {
                callback?.Invoke();
            }
        }
        /// <summary>
        /// Truncates the digit.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="digitNumber">The digit number.</param>
        /// <returns></returns>
        double TruncateDigit(double d, int digitNumber)
        {
            var power = Math.Pow(10, digitNumber);
            return Math.Truncate(d * power) / power;
        }
    }
}