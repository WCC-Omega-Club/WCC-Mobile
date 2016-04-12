using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using WCCMobile.Resources;
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
            switch (args.Position)
            {
                case 0:
                    StartActivity(typeof(PhoneBookActvity));
                    break;//start sub app at box '0' continue for each app;

                default:
                    Log.Debug("position", args.Position.ToString());
                    isReady = true;
                    break;
            }

        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();//close the app for now
        }

        protected override void OnStart()
        {
            base.OnStart();
            isReady = true;
            Log.Debug("Belief", isReady.ToString());
        }
    }
}

