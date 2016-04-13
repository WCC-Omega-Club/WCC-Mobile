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
using Java.IO;

namespace WCCMobile.Resources
{
    class YellowBookAdapter : BaseAdapter
    {
        Context context;
        static readonly string newline = System.Environment.NewLine;

        public YellowBookAdapter(Context c)
        {
            context = c;
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
                textItem.LayoutParameters = new ListView.LayoutParams(parent.Width, parent.Height / 8);
                textItem.SetPadding(30, 8, 8, 8);
            }
            else
            {
                textItem = (TextView)convertView;
            }
            string info;
            YellowBook.TryGetValue(position,out info);

            // This just ensures that the phone number xxxxxxxxxx is presented in (xxx) xxx - xxxx format.
            string info_copy = info;
            string name = info_copy.Substring(0, info_copy.IndexOf(newline));
            string phone_number = info_copy.Substring(info_copy.IndexOf(newline)+1, info_copy.LastIndexOf(newline)-info_copy.IndexOf(newline)-1);
            string phone_number_formatted = '(' + phone_number.Substring(0, 3) + ") " + phone_number.Substring(3, 3) + " - " + phone_number.Substring(6);
            string email = info_copy.Substring(info_copy.LastIndexOf(newline)+1);
            string info2 = name+newline+phone_number_formatted+newline+email;
            // End of phone number formatter

            textItem.Text = info2;
            return textItem;
        }
        static Dictionary<int, string> YellowBook = null;
        public static Dictionary<int, string> getBook
        {
            get
            {
                if (YellowBook != null) return YellowBook;
                BufferedReader reader = null;
                YellowBook = new Dictionary<int, string>();
                try
                {
                    reader = new BufferedReader(
                        new InputStreamReader(MainActivity.singleR.Assets.Open("YellowBook.txt")));

                    // do reading, usually loop until end of file reading  
                    string name;
                    string phone_number;
                    string email;
                    int key = 0;
                    while ((name = reader.ReadLine()) != null && (phone_number = reader.ReadLine()) != null && (email = reader.ReadLine()) != null)
                    {                    
                        YellowBook.Add(key++, name + newline + phone_number + newline + email);
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
                return YellowBook;
            }
        }
    }
}