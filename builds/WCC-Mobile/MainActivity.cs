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
using Android.Graphics;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WCCMobile
{
    //[Activity(MainLauncher = true, ParentActivity = typeof(MainActivity))]
    [Activity( Label = "WCC Mobile", MainLauncher = true, Icon = "@drawable/WCCMainAppIcon_57x57")]
    public class MainActivity : Activity
    {
        static Thread imgThread = null;
        static MainActivity singleton = null;
        public static ImageView ImageContainer;
        public GridView SubAppContainer;
        public static MainActivity singleR
        {
            get
            {
                return singleton;
            }
        }

        public readonly static Random LOKI = new Random();
        static bool ready = false;
        static public  bool isReady
        {
            get { return ready; }
            set { ready = value; }
        }
        static System.Collections.Generic.List<Bitmap> IMGSET = null;// = new System.Collections.Generic.List<Bitmap>();
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
        protected  override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            ImageContainer = (ImageView)FindViewById(Resource.Id.ImageContainer);
            ImageContainer.SetAdjustViewBounds(true);
            ImageContainer.SetScaleType(ImageView.ScaleType.FitCenter);
            //Log.Debug("here",IMGSRC.Count.ToString());
            //ImageContainer.SetImageBitmap(IMGSRC[LOKI.Next(0, IMGSRC.Count)]);
            SubAppContainer = (GridView)FindViewById(Resource.Id.SubAppContainer);
            singleton = this;
            SubAppContainer.Adapter = new ImageAdapter(this);
            SubAppContainer.ItemClick += StartSubApp;
            //StartService(new Intent(this,typeof(IMGTimer)));
            
            DoWork();
        }
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
                case 0: // Map
                    ImageView i = (ImageView)args.View;
                    i.SetImageBitmap(IMGSRC[LOKI.Next(0, IMGSRC.Count)]);
                    isReady = true;
                    //StartActivity(typeof(PhoneBookActvity));
                    break;//start sub app at box '0' continue for each app; 
                case 1: // Directory
                    StartActivity(typeof(PhoneBookActivity));
                    //BasicInfoActivity.setInfoTitle("Dining Services");
                    //StartActivity(typeof(BasicInfoActivity));
                    break;
                case 2: // Mail
                    StartExternalApp("com.microsoft.office.officehub", this);
                    //BasicInfoActivity.setInfoTitle("Athletics");
                    //StartActivity(typeof(BasicInfoActivity));
                    break;
                case 3: // Blackboard
                    StartExternalApp("com.blackboard.android", this);
                    //BasicInfoActivity.setInfoTitle("Counseling");
                    //StartActivity(typeof(BasicInfoActivity));
                    break;
                /* case 4: // Calendar
                    BasicInfoActivity.setInfoTitle("Student Involvement");
                    StartActivity(typeof(BasicInfoActivity));
                    break; */
                case 5: //Transit
                    BasicInfoActivity.setInfoTitle("Transit");
                    StartActivity(typeof(BasicInfoActivity));
                    //BasicInfoActivity.setInfoTitle("Career and Transfer Services");
                    //StartActivity(typeof(BasicInfoActivity));
                    break;
                /*case 6: // Library
                    //BasicInfoActivity.setInfoTitle("Financial Aid");
                    //StartActivity(typeof(BasicInfoActivity));
                    break; */
                case 7: // Athletics
                    BasicInfoActivity.setInfoTitle("Athletics");
                    StartActivity(typeof(BasicInfoActivity));
                    //BasicInfoActivity.setInfoTitle("Bursars Office");
                    //StartActivity(typeof(BasicInfoActivity));
                    break;
                case 8: // Career Services
                    BasicInfoActivity.setInfoTitle("Career and Transfer Services");
                    StartActivity(typeof(BasicInfoActivity));
                    //BasicInfoActivity.setInfoTitle("Registrar Office-Registration");
                    //StartActivity(typeof(BasicInfoActivity));
                    break;
                case 9: // Counseling
                    BasicInfoActivity.setInfoTitle("Counseling");
                    StartActivity(typeof(BasicInfoActivity));
                    //BasicInfoActivity.setInfoTitle("Transit");
                    //StartActivity(typeof(BasicInfoActivity));
                    break;
                case 10: // Dining Services
                    BasicInfoActivity.setInfoTitle("Dining Services");
                    StartActivity(typeof(BasicInfoActivity));
                    //StartExternalApp("com.blackboard.android",this);
                    break;
                case 11: // Student Involvement
                    BasicInfoActivity.setInfoTitle("Student Involvement");
                    StartActivity(typeof(BasicInfoActivity));
                    //StartExternalApp("com.microsoft.office.officehub", this);
                    break;
                case 12: // Transfer Services
                    BasicInfoActivity.setInfoTitle("Career and Transfer Services");
                    StartActivity(typeof(BasicInfoActivity));
                    break;
                default:
                    Log.Debug("position", args.Position.ToString());
                    //ImageView i = (ImageView)args.View;
                    //i.SetImageBitmap(IMGSRC[LOKI.Next(0, IMGSRC.Count)]);
                    isReady = true;// undefined buttons will just reset isReady
                    break;
            }

        }
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
        static string htmlCode = null;
        static string HTMLSRC(string urlAddress = "https://web.archive.org/web/20150306045453/http://www.sunywcc.edu/?")//http://www.sunywcc.edu/
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

