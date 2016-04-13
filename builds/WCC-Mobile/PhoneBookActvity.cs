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
using Android.Util;
using WCCMobile.Resources;
namespace WCCMobile
{
    [Activity(Label = "PhoneBook")]
    public class PhoneBookActvity : Activity
    {
        ListView PhoneBookList;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.PhoneBookLayout);
            PhoneBookList = (ListView)FindViewById(Resource.Id.PhoneBookList);
            PhoneBookList.Adapter = new YellowBookAdapter(this);
            PhoneBookList.ItemLongClick += delegate { Log.Debug("email","held"); };// replace with similar CallNumber func
            PhoneBookList.ItemClick += CallNumber;// Binds the CallNumber function to the PhoneBook ItemClick
        }
        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();
        }
        /// <summary>
        /// Gets the clicked TextView and calls a substring of the .Text property - substring is 
        /// the rest of the string after a return or newline char. Ex: '\r' , '\n'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void CallNumber(object sender, AdapterView.ItemClickEventArgs args)
        {
            CallNumber(args.Position, this);
        }
        /// <summary>
        /// Function to call numbers from YellowBook.txt - Any Activity can call this function provided
        /// they pass themselves and the key to the number in the book
        /// </summary>
        /// <param name="PhoneKey"></param>
        /// <param name="Caller"></param>
        public static void CallNumber(int PhoneKey, Activity Caller)
        {
            try
            {
                string p2Call;// person to call
                if (!YellowBookAdapter.getBook.TryGetValue(PhoneKey, out p2Call)) return;
                Android.Util.Log.Debug("Call Function", "Successfully Found :" + p2Call);
                int pos = 0;
                while (p2Call[pos] != '\r' && p2Call[pos] != '\n') ++pos;
                var uri = Android.Net.Uri.Parse("tel:" + p2Call.Substring(++pos,10));
                var intent = new Intent(Intent.ActionCall, uri);
                Caller.StartActivity(intent);
            }
            catch (Exception)
            {

            }
        }
    }
}