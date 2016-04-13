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
using WCCMobile.Resources;
using Java.IO;
namespace WCCMobile
{
    [Activity(Label = "BasicInfoActivity")]
    public class BasicInfoActivity : Activity
    {
        ListView BasicInfoList;
        static StringBuilder currenTitle = new StringBuilder();
        public static void setInfoTitle(string newTitle)
        {
            currenTitle.Length = 0;
            currenTitle.Append(newTitle);
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.BasicInfoLayout);
            ActionBar.SetIcon(ImageAdapter.Label);
            BasicInfoList = (ListView)FindViewById(Resource.Id.BasicInfoList);
            Title = currenTitle.ToString();
            BasicInfoList.Adapter = new BasicInfoAdapter(this);
            BasicInfoList.ItemClick += GoToLink;
            // Create your application here
        }
        void GoToLink(object sender, AdapterView.ItemClickEventArgs args)
        {
            //Android.Util.Log.Debug("option is", ((TextView)args.View).Text);
            try
            {
                string p2Call;// person to call
                if (!BasicInfoAdapter.getBook.TryGetValue(args.Position, out p2Call)) return;
                Android.Util.Log.Debug("Email Function", "Successfully Found :" + p2Call);
                int pos = 0;
                while (p2Call[pos] != '\r' && p2Call[pos] != '\n') ++pos;
                var uri = Android.Net.Uri.Parse(p2Call.Substring(++pos));
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            }
            catch (Exception)
            {

            }
        }

        class BasicInfoAdapter : BaseAdapter
        {
            Context context;
            static readonly string newline = System.Environment.NewLine;

            public BasicInfoAdapter(Context c)
            {
                context = c;
                if(EmailBook != null)
                EmailBook.Clear();
                EmailBook = null;
            }

            public override int Count
            {
                get { return getBook.Count; }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return null;
            }

            public override long GetItemId(int position)
            {
                return 0;
            }

            // create a new ImageView for each item referenced by the Adapter
            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                TextView textItem;
                if (convertView == null)
                {  // if it's not recycled, initialize some attributes
                    textItem = new TextView(context);
                    textItem.LayoutParameters = new ListView.LayoutParams(parent.Width, parent.Height / 10);
                    textItem.SetPadding(8, 8, 8, 8);
                }
                else
                {
                    textItem = (TextView)convertView;
                }
                string info;
                EmailBook.TryGetValue(position, out info);
                int pos = 0;
                while (info[pos] != '\r' && info[pos] != '\n') ++pos;
                textItem.Text =  info.Substring(0,pos);
                return textItem;
            }
            static Dictionary<int, string> EmailBook = null;
            public static Dictionary<int, string> getBook
            {
                get
                { 
                    if (EmailBook != null) return EmailBook;
                    BufferedReader reader = null;
                    EmailBook = new Dictionary<int, string>();
                    try
                    {
                        reader = new BufferedReader(
                            new InputStreamReader(MainActivity.singleR.Assets.Open("BasicInfoFiles/"+currenTitle + ".txt")));

                        // do reading, usually loop until end of file reading  
                        string option;
                        string web_site;
                        int key = 0;
                        while ((option = reader.ReadLine()) != null && (web_site = reader.ReadLine()) != null)
                        {
                            EmailBook.Add(key++, option + newline + web_site);
                        }
                    }
                    catch (IOException) // From (Exception e). James 04.11.16
                    {
                        //log the exception
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            try
                            {
                                reader.Close();
                            }
                            catch (IOException)
                            {
                                //log the exception
                            }
                        }
                    }
                    return EmailBook;
                }
            }
        }
    }
}