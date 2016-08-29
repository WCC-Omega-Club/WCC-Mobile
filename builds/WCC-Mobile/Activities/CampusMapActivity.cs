using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Common;
using Android.Content.PM;
using Android.Support.V4.Widget;
using WCCMobile.Models;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using System.Reflection;
using Android.Util;
using WCCMobile.Extensions;
using WCCMobile.Adapters;
using WCCMobile.ORM;

namespace WCCMobile
{

    [Activity(Label = "Campus Map",
       MainLauncher = false, ParentActivity = typeof(WCCMobile.MainActivity),
        Theme = "@style/WCCMobileTheme",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(new[] { "android.intent.action.SEARCH" }, Categories = new[] { "android.intent.category.DEFAULT" })]
    //[MetaData("android.app.searchable", Resource = "@xml/searchable")]
    public class CampusMapActivity
        : BaseActivity, IObserver<Schedule[]>, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
        const int ConnectionFailureResolutionRequest = 9000;

        CampusMapFragment mapFragment;
        ScheduleFragment scheduleFragment;

        DrawerLayout drawer;
        Android.Support.V7.App.ActionBarDrawerToggle drawerToggle;
        ListView drawerMenu;
        ListView drawerAround;
        GenericAdapter<Schedule, ScheduleViewHolder> scheduleAdapter;
        //ScheduleRepository scheduleRepository;

        DrawerAroundAdapter aroundAdapter;
        GoogleApiClient client;

        Typeface menuNormalTf, menuHighlightTf;
        private readonly string ObjectName = "CampusMapActivity";

        /// <summary>
        /// Gets the current fragment.
        /// </summary>
        /// <value>
        /// The current fragment.
        /// </value>
        Android.Support.V4.App.Fragment CurrentFragment
        {
            get
            { return new Android.Support.V4.App.Fragment[]
              {
                        mapFragment,
                        scheduleFragment,

              }.FirstOrDefault(f => f != null && f.IsAdded && f.IsVisible);
            }
        }
        /// <summary>
        /// Gets the layout resource.
        /// </summary>
        /// <value>
        /// The layout resource.
        /// </value>
        protected override int LayoutResource
        {
            get
            { return Resource.Layout.MainDrawer; }
        }
        /// <summary>
        /// Called when [create].
        /// </summary>
        /// <param name="bundle">The bundle.</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Xamarin.Insights.Initialize("0988898ef6007c41608174cd08dca7ea995c7a", this);
            Xamarin.Insights.ForceDataTransmission = true;
            Title = "WCCMobile";
            AndroidExtensions.Initialize(this);
            this.drawer = new DrawerLayout(this);
            //scheduleRepository = new ScheduleRepository();
            List<Schedule> schedule = new List<Schedule>();
            schedule.Add(new Schedule(1, DayOfWeek.Monday, new Course(1, "Data Structures 201", "Computer Science", "Learn how to become a meat space architect", "Technology Building", "101", "Steven Miller"),
                new Times(1, new TimeSpan(9, 20, 0), new TimeSpan(10, 10, 0))));
            scheduleAdapter = new GenericAdapter<Schedule, ScheduleViewHolder>(schedule, Resource.Layout.ScheduleCardView, (vw) => new ScheduleViewHolder(vw));
            
            this.drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            this.drawerToggle = new WCCMobileActionBarToggle(this,
                drawer,
                Resource.String.open_drawer,
                Resource.String.close_drawer)
            {
                OpenCallback = () =>
                {
                    SupportActionBar.Title = Title;
                    if (CurrentFragment != null)
                        CurrentFragment.HasOptionsMenu = false;
                    InvalidateOptionsMenu();
                },
                CloseCallback = () =>
                {
                    var currentFragment = CurrentFragment;
                    if (currentFragment != null)
                    {
                        SupportActionBar.Title = ((ICampusSection)currentFragment).Title;
                        currentFragment.HasOptionsMenu = true;
                    }
                    InvalidateOptionsMenu();
                },
            };
            drawer.SetDrawerShadow(Resource.Drawable.drawer_shadow, (int)GravityFlags.Left);
            drawer.AddDrawerListener(drawerToggle);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            ScheduleObserver.Instance.Subscribe(this);
            // ScheduleManager.ScheduleChanged += (sender, e) => aroundAdapter.Refresh();

            drawerMenu = FindViewById<ListView>(Resource.Id.left_drawer);
            drawerMenu.AddFooterView(new Android.Support.V4.Widget.Space(this));
            drawerMenu.ItemClick += HandleSectionItemClick;
            menuNormalTf = Typeface.Create(Resources.GetString(Resource.String.menu_item_fontFamily),
                TypefaceStyle.Normal);

            drawerMenu.Adapter = new DrawerMenuAdapter(this);

            drawerAround = FindViewById<ListView>(Resource.Id.left_drawer_around);
            drawerAround.ItemClick += HandleAroundItemClick;
            drawerAround.Adapter = aroundAdapter = new DrawerAroundAdapter(this);

            drawerMenu.SetItemChecked(0, true);

            if (CheckGooglePlayServices())
            {
                client = CreateApiClient();
                SwitchTo(mapFragment = new CampusMapFragment(this));
                SupportActionBar.Title = ((ICampusSection)mapFragment).Title;
            }
        }
        /// <summary>
        /// Creates the API client.
        /// </summary>
        /// <returns></returns>
        private GoogleApiClient CreateApiClient()
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name);
            return new GoogleApiClient.Builder(this, this, this)
            .AddApi(LocationServices.API)
            .Build();
        }
        /// <summary>
        /// Handles the around item click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="AdapterView.ItemClickEventArgs"/> instance containing the event data.</param>
        void HandleAroundItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (mapFragment != null)
            {
                Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name + $" with args: sender: {sender}, eventargs:{e})");

                drawer.CloseDrawers();
                mapFragment.CenterAndOpenOnMap(e.Id,
                    zoom: 17,
                    animDurationID: Android.Resource.Integer.ConfigLongAnimTime);
            }
        }
        /// <summary>
        /// Handles the section item click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="AdapterView.ItemClickEventArgs"/> instance containing the event data.</param>
        void HandleSectionItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            switch (e.Position)
            {
                case 0:
                    if (mapFragment == null)
                        mapFragment = new CampusMapFragment(this);
                    SwitchTo(mapFragment);
                    break;
                case 1:
                    if (scheduleFragment == null)
                    {

                    }
                    SwitchTo(scheduleFragment);
                    break;
                case 2:
                    //StartActivity(new Intent(this, typeof(SettingsActivity)));
                    var data = new Dictionary<string, string>();
                    data.Add("Section", "Settings");
                    Xamarin.Insights.Track("Navigated", data);
                    break;


                default:
                    return;
            }
            SetSelectedMenuIndex(e.Position);
            drawerMenu.SetItemChecked(e.Position, true);
            drawer.CloseDrawers();
        }
        /// <summary>
        /// Switches to the instance of <see cref="Android.Support.V4.App.Fragment"/>.
        /// </summary>
        /// <param name="fragment">The fragment.</param>
        private void SwitchTo(Android.Support.V4.App.Fragment fragment)
        {
            if (fragment.IsVisible)
                return;
            var section = fragment as ICampusSection;
            if (section == null)
                return;
            var name = section.Name;
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name);

            var data = new Dictionary<string, string>();
            data.Add("Section", section.Name);
            Xamarin.Insights.Track("Navigated", data);

            var t = SupportFragmentManager.BeginTransaction();
            var currentFragment = CurrentFragment;
            if (currentFragment == null)
            {
                t.Add(Resource.Id.content_frame, fragment, name);
            }
            else
            {
                t.SetCustomAnimations(Resource.Animator.frag_slide_in,
                    Resource.Animator.frag_slide_out);
                var existingFragment = SupportFragmentManager.FindFragmentByTag(name);
                if (existingFragment != null)
                    existingFragment.View.BringToFront();
                currentFragment.View.BringToFront();
                t.Hide(CurrentFragment);
                if (existingFragment != null)
                {
                    t.Show(existingFragment);
                }
                else
                {
                    t.Add(Resource.Id.content_frame, fragment, name);

                }
                section.RefreshData();
            }
            t.Commit();
        }
        /// <summary>
        /// Sets the index of the selected menu.
        /// </summary>
        /// <param name="pos">The position.</param>
        void SetSelectedMenuIndex(int pos)
        {
            for (int i = 0; i < 2; i++)
            {
                var view = drawerMenu.GetChildAt(i);
                var text = view.FindViewById<TextView>(Resource.Id.roomText);
                if (i == pos)
                    text.SetTypeface(text.Typeface, TypefaceStyle.Bold);
                else
                    text.SetTypeface(text.Typeface, TypefaceStyle.Normal);

            }
        }
        /// <summary>
        /// Called when activity start-up is complete (after <c><see cref="M:Android.App.Activity.OnStart" /></c>
        /// and <c><see cref="M:Android.App.Activity.OnRestoreInstanceState(Android.OS.Bundle)" /></c> have been called).
        /// </summary>
        /// <param name="savedInstanceState">If the activity is being re-initialized after
        /// previously being shut down then this Bundle contains the data it most
        /// recently supplied in <c><see cref="M:Android.App.Activity.OnSaveInstanceState(Android.OS.Bundle)" /></c>.  <format type="text/html"><b><i>Note: Otherwise it is null.</i></b></format></param>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Called when activity start-up is complete (after <c><see cref="M:Android.App.Activity.OnStart" /></c>
        /// and <c><see cref="M:Android.App.Activity.OnRestoreInstanceState(Android.OS.Bundle)" /></c> have been called).  Applications will
        /// generally not implement this method; it is intended for system
        /// classes to do final initialization after application code has run.
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <i>Derived classes must call through to the super class's
        /// implementation of this method.  If they do not, an exception will be
        /// thrown.</i>
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/app/Activity.html#onPostCreate(android.os.Bundle)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        /// <altmember cref="M:Android.App.Activity.OnCreate(Android.OS.Bundle)" />
        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            drawerToggle.SyncState();
        }
        /// <summary>
        /// Called by the system when the device configuration changes while your
        /// activity is running.
        /// </summary>
        /// <param name="newConfig">The new device configuration.</param>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Called by the system when the device configuration changes while your
        /// activity is running.  Note that this will <i>only</i> be called if
        /// you have selected configurations you would like to handle with the
        /// <c><see cref="F:Android.Resource.Attribute.ConfigChanges" /></c> attribute in your manifest.  If
        /// any configuration change occurs that is not selected to be reported
        /// by that attribute, then instead of reporting it the system will stop
        /// and restart the activity (to have it launched with the new
        /// configuration).
        /// </para>
        /// <para tool="javadoc-to-mdoc">At the time that this function has been called, your Resources
        /// object will have been updated to return resource values matching the
        /// new configuration.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/app/Activity.html#onConfigurationChanged(android.content.res.Configuration)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            drawerToggle.OnConfigurationChanged(newConfig);
        }
        /// <summary>
        /// This hook is called whenever an item in your options menu is selected.
        /// </summary>
        /// <param name="item">The menu item that was selected.</param>
        /// <returns>
        /// To be added.
        /// </returns>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">This hook is called whenever an item in your options menu is selected.
        /// The default implementation simply returns false to have the normal
        /// processing happen (calling the item's Runnable or sending a message to
        /// its Handler as appropriate).  You can use this method for any items
        /// for which you would like to do processing without those other
        /// facilities.
        /// </para>
        /// <para tool="javadoc-to-mdoc">Derived classes should call through to the base class for it to
        /// perform the default menu handling.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/app/Activity.html#onOptionsItemSelected(android.view.MenuItem)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        /// <altmember cref="M:Android.App.Activity.OnCreateOptionsMenu(Android.Views.IMenu)" />
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (drawerToggle.OnOptionsItemSelected(item))
                return true;
            return base.OnOptionsItemSelected(item);
        }
        /// <summary>
        /// This is called for activities that set launchMode to "singleTop" in
        /// their package, or if a client used the <c><see cref="F:Android.Content.ActivityFlags.SingleTop" /></c>
        /// flag when calling <c><see cref="M:Android.Content.ContextWrapper.StartActivity(Android.Content.Intent)" /></c>.
        /// </summary>
        /// <param name="intent">The new intent that was started for the activity.</param>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">This is called for activities that set launchMode to "singleTop" in
        /// their package, or if a client used the <c><see cref="F:Android.Content.ActivityFlags.SingleTop" /></c>
        /// flag when calling <c><see cref="M:Android.Content.ContextWrapper.StartActivity(Android.Content.Intent)" /></c>.  In either case, when the
        /// activity is re-launched while at the top of the activity stack instead
        /// of a new instance of the activity being started, onNewIntent() will be
        /// called on the existing instance with the Intent that was used to
        /// re-launch it.
        /// </para>
        /// <para tool="javadoc-to-mdoc">An activity will always be paused before receiving a new intent, so
        /// you can count on <c><see cref="M:Android.App.Activity.OnResume" /></c> being called after this method.
        /// </para>
        /// <para tool="javadoc-to-mdoc">Note that <c><see cref="P:Android.App.Activity.Intent" /></c> still returns the original Intent.  You
        /// can use <c><see cref="P:Android.App.Activity.Intent" /></c> to update it to this new Intent.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/app/Activity.html#onNewIntent(android.content.Intent)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        /// <altmember cref="P:Android.App.Activity.Intent" />
        /// <altmember cref="P:Android.App.Activity.Intent" />
        /// <altmember cref="M:Android.App.Activity.OnResume" />
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            try
            {
                if (mapFragment != null && mapFragment.IsVisible)
                    mapFragment.OnSearchIntent(intent);
            }
            catch
            {
            }
        }
        /// <summary>
        /// Called after <c><see cref="M:Android.App.Activity.OnCreate(Android.OS.Bundle)" /></c> &amp;mdash; or after <c><see cref="M:Android.App.Activity.OnRestart" /></c> when
        /// the activity had been stopped, but is now again being displayed to the
        /// user.
        /// </summary>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Called after <c><see cref="M:Android.App.Activity.OnCreate(Android.OS.Bundle)" /></c> &amp;mdash; or after <c><see cref="M:Android.App.Activity.OnRestart" /></c> when
        /// the activity had been stopped, but is now again being displayed to the
        /// user.  It will be followed by <c><see cref="M:Android.App.Activity.OnResume" /></c>.
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <i>Derived classes must call through to the super class's
        /// implementation of this method.  If they do not, an exception will be
        /// thrown.</i>
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/app/Activity.html#onStart()" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        /// <altmember cref="M:Android.App.Activity.OnCreate(Android.OS.Bundle)" />
        /// <altmember cref="M:Android.App.Activity.OnStop" />
        /// <altmember cref="M:Android.App.Activity.OnResume" />
        protected override void OnStart()
        {
            if (client != null)
                client.Connect();
            base.OnStart();
        }
        /// <summary>
        /// Called when you are no longer visible to the user.
        /// </summary>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Called when you are no longer visible to the user.  You will next
        /// receive either <c><see cref="M:Android.App.Activity.OnRestart" /></c>, <c><see cref="M:Android.App.Activity.OnDestroy" /></c>, or nothing,
        /// depending on later user activity.
        /// </para>
        /// <para tool="javadoc-to-mdoc">Note that this method may never be called, in low memory situations
        /// where the system does not have enough memory to keep your activity's
        /// process running after its <c><see cref="M:Android.App.Activity.OnPause" /></c> method is called.
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <i>Derived classes must call through to the super class's
        /// implementation of this method.  If they do not, an exception will be
        /// thrown.</i>
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/app/Activity.html#onStop()" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        /// <altmember cref="M:Android.App.Activity.OnRestart" />
        /// <altmember cref="M:Android.App.Activity.OnResume" />
        /// <altmember cref="M:Android.App.Activity.OnSaveInstanceState(Android.OS.Bundle)" />
        /// <altmember cref="M:Android.App.Activity.OnDestroy" />
        protected override void OnStop()
        {
            if (client != null)
                client.Disconnect();
            base.OnStop();
        }
        /// <summary>
        /// Called when an activity you launched exits, giving you the requestCode
        /// you started it with, the resultCode it returned, and any additional
        /// data from it.
        /// </summary>
        /// <param name="requestCode">The integer request code originally supplied to
        /// startActivityForResult(), allowing you to identify who this
        /// result came from.</param>
        /// <param name="resultCode">The integer result code returned by the child activity
        /// through its setResult().</param>
        /// <param name="data">An Intent, which can return result data to the caller
        /// (various data can be attached to Intent "extras").</param>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Called when an activity you launched exits, giving you the requestCode
        /// you started it with, the resultCode it returned, and any additional
        /// data from it.  The <format type="text/html"><var>resultCode</var></format> will be
        /// <c><see cref="F:Android.App.Result.Canceled" /></c> if the activity explicitly returned that,
        /// didn't return any result, or crashed during its operation.
        /// </para>
        /// <para tool="javadoc-to-mdoc">You will receive this call immediately before onResume() when your
        /// activity is re-starting.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/app/Activity.html#onActivityResult(int, int, android.content.Intent)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        /// <altmember cref="M:Android.App.Activity.StartActivityForResult(Android.Content.Intent, System.Int32)" />
        /// <altmember cref="M:Android.App.Activity.CreatePendingResult(System.Int32, Android.Content.Intent, Android.Content.Intent)" />
        /// <altmember cref="M:Android.App.Activity.SetResult(Android.App.Result)" />
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == ConnectionFailureResolutionRequest)
            {
                if (resultCode == Result.Ok && CheckGooglePlayServices())
                {
                    if (client == null)
                    {
                        client = CreateApiClient();
                        client.Connect();
                    }
                    SwitchTo(mapFragment = new CampusMapFragment(this));
                }
                else
                    Finish();
            }
            else
            {
                base.OnActivityResult(requestCode, resultCode, data);
            }
        }
        /// <summary>
        /// Checks the google play services.
        /// </summary>
        /// <returns></returns>
        private bool CheckGooglePlayServices()
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name);

            var result = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (result == ConnectionResult.Success)
                return true;
            var dialog = GoogleApiAvailability.Instance.GetErrorDialog(this, result,
                    ConnectionFailureResolutionRequest);
            if (dialog != null)
            {
                var errorDialog = new ErrorDialogFragment { Dialog = dialog };
                errorDialog.Show(SupportFragmentManager, "Google Services Updates");
                return false;
            }

            Finish();
            return false;
        }

        #region IObserver implementation

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Schedule[] value)
        {
            if (client == null || !client.IsConnected)
                return;
            var location = LocationServices.FusedLocationApi.GetLastLocation(client);
            if (location == null)
                return;
            var events = ScheduleObserver.GetEventsAround(value,
                      new Android.Gms.Maps.Model.LatLng (location.Latitude, location.Longitude),
                      minDistance: 1,
                      maxItems: 4);
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name);

            RunOnUiThread(() => aroundAdapter.SetSchedule(events));
        }

        #endregion

        public void OnConnected(Bundle p0)
        {
            Log.Debug(ObjectName, MethodInfo.GetCurrentMethod().Name);
            if (ScheduleObserver.Instance.LastScheduleItems != null)
                OnNext(ScheduleObserver.Instance.LastScheduleItems);
        }
        /// <summary>
        /// Called when [disconnected].
        /// </summary>
        public void OnDisconnected()
        {

        }
        /// <summary>
        /// Called when [connection failed].
        /// </summary>
        /// <param name="p0">The p0.</param>
        public void OnConnectionFailed(ConnectionResult p0)
        {

        }
        /// <summary>
        /// Called when [connection suspended].
        /// </summary>
        /// <param name="reason">The reason.</param>
        public void OnConnectionSuspended(int reason)
        {

        }
    }


    class MyViewHolder : Java.Lang.Object
    {
        public TextView Title { get; set; }
    }

    class DrawerMenuAdapter : BaseAdapter
    {
        Tuple<int, string>[] sections = {
            Tuple.Create(Resource.Drawable.ic_drawer_map, "Campus Map"),
            Tuple.Create(Resource.Drawable.ic_drawer_star, "Schedule"),
            Tuple.Create(Resource.Drawable.ic_about, "Feed"),
            Tuple.Create(Resource.Drawable.ic_map_footer_location_active, "Navigation"),
        };

        Context context;

        public DrawerMenuAdapter(Context context)
        {
            this.context = context;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return new Java.Lang.String(sections[position - 1].Item2);
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            MyViewHolder holder = null;

            if (view != null)
                holder = view.Tag as MyViewHolder;

            if (holder == null)
            {
                holder = new MyViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                view = inflater.Inflate(Resource.Layout.DrawerItemLayout, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.text);
                view.Tag = holder;

            }

            if (position == 0 && convertView == null)
                holder.Title.SetTypeface(holder.Title.Typeface, TypefaceStyle.Bold);
            else
                holder.Title.SetTypeface(holder.Title.Typeface, TypefaceStyle.Normal);


            holder.Title.Text = sections[position].Item2;
            holder.Title.SetCompoundDrawablesWithIntrinsicBounds(sections[position].Item1, 0, 0, 0);

            return view;
        }

        public override int Count
        {
            get
            {
                return sections.Length;
            }
        }

        public override bool IsEnabled(int position)
        {
            return true;
        }

        public override bool AreAllItemsEnabled()
        {
            return false;
        }
    }

    class DrawerAroundAdapter : BaseAdapter
{
    Context context;

    Schedule[] schedule = null;
    ScheduleManager manager;
    HashSet<int> currentSchedule;

    Drawable starDrawable;

    public DrawerAroundAdapter(Context context)
    {
        this.context = context;
        this.manager =  ScheduleManager.Obtain(context);
        this.starDrawable = XamSvg.SvgFactory.GetDrawable(context.Resources, Resource.Raw.star_depressed);
        this.currentSchedule = new HashSet<Int32>();
        LoadSchedule();
    }

    public void SetSchedule(Schedule[] schedule)
    {
        this.schedule = schedule;
        NotifyDataSetChanged();
    }

    async void LoadSchedule()
    {
     
        NotifyDataSetChanged();
    }

    public void Refresh()
    {
        LoadSchedule();
    }

    public override Java.Lang.Object GetItem(int position)
    {
        return new Java.Lang.String(schedule[position].Course.Name);
    }

    public override long GetItemId(int position)
    {
        return schedule[position].TimesId;
    }

    public override View GetView(int position, View convertView, ViewGroup parent)
    {
        var view = convertView;
        if (view == null)
        {
            var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
            view = inflater.Inflate(Resource.Layout.DrawerAroundItem, parent, false);
        }

        var navigate = view.FindViewById<ImageView>(Resource.Id.aroundStar);
        var scheduleItemName = view.FindViewById<TextView>(Resource.Id.aroundStation1);
        var scheduleItemSecondName = view.FindViewById<TextView>(Resource.Id.aroundStation2);
        var timeSlot = view.FindViewById<TextView>(Resource.Id.aroundBikes);
        var description = view.FindViewById<TextView>(Resource.Id.aroundRacks);

        var scheduleItem = schedule[position];
        navigate.SetImageDrawable(starDrawable);
        navigate.Visibility = currentSchedule.Contains(scheduleItem.TimesId) ? ViewStates.Visible : ViewStates.Invisible;

        scheduleItemName.Text = scheduleItem.Course.Major;
        scheduleItemSecondName.Text = scheduleItem.Course.Name;
        timeSlot.Text = scheduleItem.Times.StartTime + "-" + scheduleItem.Times.EndTime;
        description.Text = scheduleItem.Course.Description;

        return view;
    }

    public override int Count
    {
        get
        {
            return schedule == null ? 0 : schedule.Length;
        }
    }
        /// <summary>
        /// Returns true if the item at the specified position is not a separator.
        /// </summary>
        /// <param name="position">Index of the item</param>
        /// <returns>
        /// To be added.
        /// </returns>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Returns true if the item at the specified position is not a separator.
        /// (A separator is a non-selectable, non-clickable item).
        /// The result is unspecified if position is invalid. An <c><see cref="T:Java.Lang.ArrayIndexOutOfBoundsException" /></c>
        /// should be thrown in that case for fast failure.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/widget/BaseAdapter.html#isEnabled(int)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        public override bool IsEnabled(int position)
    {
        return true;
    }
        /// <summary>
        /// Ares all items enabled.
        /// </summary>
        /// <returns></returns>
        public override bool AreAllItemsEnabled()
    {
        return false;
    }
}

    class ErrorDialogFragment : Android.Support.V4.App.DialogFragment
{
    public new Dialog Dialog
    {
        get;
        set;
    }
        /// <summary>
        /// Called when [create dialog].
        /// </summary>
        /// <param name="savedInstanceState">State of the saved instance.</param>
        /// <returns></returns>
        public override Dialog OnCreateDialog(Bundle savedInstanceState)
    {
        return Dialog;
    }
}

    class WCCMobileActionBarToggle : Android.Support.V7.App.ActionBarDrawerToggle
{   /// <summary>
    /// 
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="drawer"></param>
    /// <param name="openDrawerContentRes"></param>
    /// <param name="closeDrawerContentRest"></param>
    public WCCMobileActionBarToggle(Activity activity,
                            DrawerLayout drawer,
                            int openDrawerContentRes,
                            int closeDrawerContentRest)
        : base(activity, drawer, openDrawerContentRes, closeDrawerContentRest)
    {
    }
        /// <summary>
        /// Gets or sets the open callback.
        /// </summary>
        /// <value>
        /// The open callback.
        /// </value>
        public Action OpenCallback
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the close callback.
        /// </summary>
        /// <value>
        /// The close callback.
        /// </value>
        public Action CloseCallback
        {
            get;
            set;
        }
        /// <summary>
        /// Called when [drawer opened].
        /// </summary>
        /// <param name="drawerView">The drawer view.</param>
        public override void OnDrawerOpened(View drawerView)
        {
            base.OnDrawerOpened(drawerView);
            OpenCallback?.Invoke();
        }
        /// <summary>
        /// Called when [drawer closed].
        /// </summary>
        /// <param name="drawerView">The drawer view.</param>
        public override void OnDrawerClosed(View drawerView)
        {
            base.OnDrawerClosed(drawerView);
            CloseCallback?.Invoke();
        }
}
}