using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Util;
using WCCMobile.Resources;
using Android.Graphics;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Android.Support.V7.App;
using HockeyApp.Android;
namespace WCCMobile
{
    
    [Activity( Label = "WCC Mobile", MainLauncher = true, Icon = "@drawable/WCCMainAppIcon_57x57", Theme ="@style/WCCMobileTheme")]
    public class MainActivity : AppCompatActivity
    {
        static Thread imgThread = null;
        static MainActivity singleton = null;
        public static ImageView ImageContainer;
        public GridView SubAppContainer;
        /// <summary>
        /// Gets the singleton instance of the MainActivity.
        /// </summary>
        /// <value>
        /// The single r.
        /// </value>
        public static MainActivity singleR
        {
            get
            {
                return singleton;
            }
        }
        /// <summary>
        /// The random number generator.
        /// </summary>
        public readonly static Random LOKI = new Random();
        /// <summary>
        /// The backing field ready
        /// </summary>
        static bool ready = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is ready.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </value>
        static public  bool isReady
        {
            get { return ready; }
            set { ready = value; }
        }
        /// <summary>
        /// The set of images to be horizontally scrolled through on subapp menu.
        /// </summary>
        static System.Collections.Generic.List<Bitmap> IMGSET = null;// = new System.Collections.Generic.List<Bitmap>();        
        /// <summary>
        /// Gets the set of images to be horizontally scrolled through on subapp menu.
        /// </summary>
        public static System.Collections.Generic.List<Bitmap> IMGSRC
        {
            get
            {
                if (IMGSET == null)
                {
                    IMGSET = new System.Collections.Generic.List<Bitmap>();
                    int bg = -1;
                    int eg = -1;
                    foreach (Match m in Regex.Matches(HTMLSRC(), "(http(s?):)|([/|.|w|s])*.(?:jpg)"))//|gif|png
                    {
                        if (m.Value.Contains("http") && eg < 0) bg = m.Index;
                        else if (m.Value.Contains("jpg") && bg >= 0) eg = m.Index;
                        else bg = eg = -1;
                        if (bg >= 0 && eg >= 0)
                        {
                            string nextSrc = htmlCode.Substring(bg, ((eg + 5) - bg));
                            while (!char.IsLetter(nextSrc[nextSrc.Length - 1])) nextSrc = nextSrc.Remove(nextSrc.Length - 1, 1);
                            Log.Debug("got ->", nextSrc);
                            bg = eg = -1;
                            Bitmap nextImage;
                            if((nextImage = GetImageBitmapFromUrl(nextSrc)) != null)
                                IMGSET.Add(nextImage);
                                
                        }
                    }
                }

                return IMGSET;
            }
        }
        /// <summary>
        /// Called when [create].
        /// </summary>
        /// <param name="bundle">The bundle.</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            CrashManager.Register(this);
            SetContentView(Resource.Layout.Main);
            ImageContainer = (ImageView)FindViewById(Resource.Id.ImageContainer);
            ImageContainer.SetAdjustViewBounds(true);
            ImageContainer.SetScaleType(ImageView.ScaleType.FitCenter);
            ImageContainer.SetScaleType(ImageView.ScaleType.FitXy);
            ImageContainer.SetImageBitmap(IMGSRC[LOKI.Next(0, IMGSRC.Count)]);

            LinearLayout.LayoutParams l = new LinearLayout.LayoutParams(Android.Views.ViewGroup.LayoutParams.MatchParent, 400);
            l.Height = (int)Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Dip, 205, Resources.DisplayMetrics);
            ImageContainer.LayoutParameters = l;

            SubAppContainer = (GridView)FindViewById(Resource.Id.SubAppContainer);
            singleton = this;
            SubAppContainer.Adapter = new ImageAdapter(this);
            SubAppContainer.ItemClick += StartSubApp;
            DoWork();
        }
        /// <summary>
        /// Does the work.
        /// </summary>
        public void DoWork()
        {
            
             Thread t = new Thread(() => {
               // imgThread = t;
                int die = LOKI.Next(0, IMGSRC.Count);
                 MainActivity mine = MainActivity.singleR;
                while (MainActivity.singleR == mine && MainActivity.singleR != null)
                {
                    RunOnUiThread(() =>
                    {
                        die = (++die == IMGSRC.Count ? 0 : die);
                        {
                            ImageContainer.SetImageBitmap(IMGSRC[die]);
                            //Log.Debug("position", "workaa");
                        }
                    });
                    Thread.Sleep(6000);
                }
                Log.Debug("Thread", "DONE!");
            }
            );
            t.Start();
        }
        /// <summary>
        /// Start an activity that matches the case where case = args.Position in the GridView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void StartSubApp (object sender, AdapterView.ItemClickEventArgs args)
        {
            
            //Log.Debug("WebCode",getHTML());
            if (!isReady) return; // each app must be completed first
            isReady = false;
            ImageAdapter.Label = args.Position;
            switch (args.Position)
            {

                //case 0: // Map
                    //ImageView i = (ImageView)args.View;
                    //i.SetImageBitmap(IMGSRC[LOKI.Next(0, IMGSRC.Count)]);
                    //isReady = true;
                    
                    //break;//start sub app at box '0' continue for each app; 
                case 0: // Map
                    StartActivity(typeof(CampusMapActivity));
                    break;//start sub app at box '0' continue for each app; 
                case 1: // Directory
                    StartActivity(typeof(PhoneBookActivity));
                    break;
                case 2: // Mail
                    StartExternalApp("com.microsoft.office.officehub", this);
                    break;
                case 3: // Blackboard
                    StartExternalApp("com.blackboard.android", this);
                    break;
                 case 4: // Calendar
                    AlertNotImplemented();
                    isReady = true;
                    //BasicInfoActivity.setInfoTitle("Student Involvement");
                    //StartActivity(typeof(BasicInfoActivity));
                    break; 
                case 5: //Transit
                    BasicInfoActivity.setInfoTitle("Transit");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                case 6: // Library
                    //AlertNotImplemented();
                    //isReady = true;
                    ////TODO: Change android icon (on back pressed)
                    StartActivity(typeof(CourseActivity));
                    //BasicInfoActivity.setInfoTitle("Financial Aid");
                    //StartActivity(typeof(BasicInfoActivity));
                    break; 
                case 7: // Athletics
                    BasicInfoActivity.setInfoTitle("Athletics");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                case 8: // Career Services
                    BasicInfoActivity.setInfoTitle("Career and Transfer Services");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                case 9: // Counseling
                    BasicInfoActivity.setInfoTitle("Counseling");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                case 10: // Dining Services
                    BasicInfoActivity.setInfoTitle("Dining Services");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                case 11: // Student Involvement
                    BasicInfoActivity.setInfoTitle("Student Involvement");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                case 12: // Transfer Services
                    BasicInfoActivity.setInfoTitle("Career and Transfer Services");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                default:
                    AlertNotImplemented();
                    //Log.Debug("position", args.Position.ToString());

                    //isReady = true;// undefined buttons will just reset isReady
                    break;
            }

        }
        /// <summary>
        /// Starts the sub application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static   void StartSubApp(int args)
        {
            //ImageView iv = (ImageView)args.View;

            if (!isReady) return; // each app must be completed first
            isReady = false;
            ImageAdapter.Label = args;
            switch (args)
            {
                case 0:
                    MainActivity.singleR.StartActivity(typeof(PhoneBookActivity));
                    break;//start sub app at box '0' continue for each app;
                case 1:
                    BasicInfoActivity.setInfoTitle("Dining Services");
                    MainActivity.singleR.StartActivity(typeof(BasicInfoActivity));
                    break;
                case 2:
                    BasicInfoActivity.setInfoTitle("Athletics");
                    MainActivity.singleR.StartActivity(typeof(BasicInfoActivity));
                    break;
                case 3:
                    BasicInfoActivity.setInfoTitle("Counseling");
                    MainActivity.singleR.StartActivity(typeof(BasicInfoActivity));
                    break;
                case 4:
                    BasicInfoActivity.setInfoTitle("Student Involvement");
                    MainActivity.singleR.StartActivity(typeof(BasicInfoActivity));
                    break;
                case 5:
                    BasicInfoActivity.setInfoTitle("Career and Transfer Services");
                    MainActivity.singleR.StartActivity(typeof(BasicInfoActivity));
                    break;
                case 6:
                    BasicInfoActivity.setInfoTitle("Financial Aid");
                    MainActivity.singleR.StartActivity(typeof(BasicInfoActivity));
                    break;
                case 7:
                    BasicInfoActivity.setInfoTitle("Bursars Office");
                    MainActivity.singleR.StartActivity(typeof(BasicInfoActivity));
                    break;
                case 8:
                    BasicInfoActivity.setInfoTitle("Registrar Office-Registration");
                    MainActivity.singleR.StartActivity(typeof(BasicInfoActivity));
                    break;
                case 9:
                    BasicInfoActivity.setInfoTitle("Transit");
                    MainActivity.singleR.StartActivity(typeof(BasicInfoActivity));
                    break;
                case 10:
                    StartExternalApp("com.blackboard.android", MainActivity.singleR);
                    break;
                case 11:
                    StartExternalApp("com.microsoft.office.officehub", MainActivity.singleR);
                    break;
                default:
                    Log.Debug("position", args.ToString());
                    isReady = true;// undefined buttons will just reset isReady
                    break;
            }

        }
        /// <summary>
        /// Alerts the not implemented.
        /// </summary>
        public void AlertNotImplemented()
        {
            Android.Support.V7.App.AlertDialog.Builder a = new Android.Support.V7.App.AlertDialog.Builder(this);
            Android.Support.V7.App.AlertDialog alertNotImpl = a.Create();
            alertNotImpl.DismissEvent += delegate { isReady = true; };
            alertNotImpl.SetTitle("Sorry!");
            alertNotImpl.SetMessage("This feature has not yet been implemented.\n\nPlease try again later.");
            alertNotImpl.SetButton(1, "OK", (sender, args) =>
            {
                Toast.MakeText(this, "Action Cancelled", ToastLength.Short).Show();
                isReady = true;
            });
            alertNotImpl.Show();
        }
        /// <summary>
        ///  Can be used by any sub App to launch external 3rd Party apps pass 
        ///  the Activity that wishes to launch and the package name
        ///  to get the package name search the app name on : http://apk-dl.com/ 
        /// </summary>
        /// <param name="appPackageName"></param>
        /// <param name="Caller"></param>
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
        /// <summary>
        /// Called when the activity has detected the user's press of the back
        /// key.
        /// </summary>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Called when the activity has detected the user's press of the back
        /// key.  The default implementation simply finishes the current activity,
        /// but you can override this to do whatever you want.
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/app/Activity.html#onBackPressed()" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 5" />
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
            Log.Debug("MainActivity", isReady.ToString());
        }
        /// <summary>
        /// Gets the image bitmap from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;
            using (var webClient = new System.Net.WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }
            return imageBitmap;
        }
        /// <summary>
        /// The HTML code
        /// </summary>
        static string htmlCode = null;//https://web.archive.org/web/20150306045453/http://www.sunywcc.edu/?        
        /// <summary>
        /// </summary>
        /// <param name="urlAddress">The URL address.</param>
        /// <returns></returns>
        static string HTMLSRC(string urlAddress = "https://web.archive.org/web/20150306045453/http://www.sunywcc.edu/")//http://www.sunywcc.edu/
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
                return htmlCode = data;
            }
            return string.Empty;
        }
        /// <summary>
        /// Gets the HTML.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public string getHTML(string url = "http://www.sunywcc.edu/")
        {
            //Create request for given url
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

            //Create response-object
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //Take response stream
            StreamReader sr = new StreamReader(response.GetResponseStream());

            //Read response stream (html code)
            string html = sr.ReadToEnd();

            //Close streamreader and response
            sr.Close();
            response.Close();

            //return source
            return html;
        }
    
    }
    
}

