using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using WCCMobile.Resources;
using Java.IO;
using Android.Graphics.Pdf;
namespace WCCMobile
{
    [Activity(Label = "WCC Mobile", MainLauncher = true, Icon = "@drawable/WCCMainAppIcon_57x57")]
    public class MainActivity : Activity
    {
        static MainActivity singleton = null;
        public GridView SubAppContainer;
        public static MainActivity singleR
        {
            get
            {
                return singleton;
            }
        }
        bool ready = false;
        public  bool isReady
        {
            get { return ready; }
            set { ready = value; }
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
           
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            SubAppContainer = (GridView)FindViewById(Resource.Id.SubAppContainer);
            singleton = this;
            SubAppContainer.Adapter = new ImageAdapter(this);
            SubAppContainer.ItemClick += StartSubApp;
            
            
        }
/// <summary>
/// Start an activity that matches the case where case = args.Position in the GridView
/// </summary>
/// <param name="sender"></param>
/// <param name="args"></param>
void StartSubApp (object sender, AdapterView.ItemClickEventArgs args)
        {
            if (!isReady) return; // each app must be completed first
            isReady = false;
            ImageAdapter.Label = args.Position;
            switch (args.Position)
            {
                case 0:
                    StartActivity(typeof(PhoneBookActvity));
                    break;//start sub app at box '0' continue for each app;
                case 1:
                    BasicInfoActivity.setInfoTitle("DiningServices");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                case 2:
                    BasicInfoActivity.setInfoTitle("Athletics");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                case 3:
                    BasicInfoActivity.setInfoTitle("Counseling");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                case 4:
                    BasicInfoActivity.setInfoTitle("Student Involvement");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                case 5:
                    BasicInfoActivity.setInfoTitle("Career and Transfer Services");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                case 6:
                    StartExternalApp("com.blackboard.android",this);
                    break;
                default:
                    Log.Debug("position", args.Position.ToString());
                    isReady = true;// undefined buttons will just reset isReady
                    break;
            }

        }
        public static void StartExternalApp(string appPackageName, Activity Caller)
        {
            Log.Debug("Attempting to start app via package : ", appPackageName); 
            try { Caller.StartActivity(Caller.PackageManager.GetLaunchIntentForPackage(appPackageName)); }
            catch
            {
                try
                { 
                    Log.Debug("Attempting to enter the Store App via Google Play", appPackageName);
                    Caller.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("market://details?id=" + appPackageName)));
                }
                catch (Exception)//android.content.ActivityNotFoundException anfe
                {
                    Log.Debug("Attempting to enter the Store App via Browser", appPackageName);
                    Caller.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("http://play.google.com/store/apps/details?id=" + appPackageName)));
                }
            }
        }
        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();//close the app for now
        }
        /// <summary>
        /// Called after OnCreate and Also when a SubbApp closes - resets isReady 
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            isReady = true;
            Log.Debug("Belief", isReady.ToString());
        }
    }
}

