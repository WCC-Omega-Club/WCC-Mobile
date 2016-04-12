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
    [Activity(Label = "PhoneBookActvity")]
    public class PhoneBookActvity : Activity
    {
        ListView PhoneBookList;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.PhoneBookLayout);
            PhoneBookList = (ListView)FindViewById(Resource.Id.PhoneBookList);
            PhoneBookList.Adapter = new YellowBookAdapter(this);
            PhoneBookList.ItemClick += CallNumber;
            // Create your application here
        }
        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();
        }
        void CallNumber(object sender, AdapterView.ItemClickEventArgs args)
        {
            TextView callbox = (TextView)args.View;
            try
            {
                int pos = 0;
                while (callbox.Text[pos] != '\r' && callbox.Text[pos] != '\n') ++pos;
                //Log.Debug("Info is :", callbox.Text.Substring(++pos));// 
                var uri = Android.Net.Uri.Parse("tel:" + callbox.Text.Substring(++pos));
                var intent = new Intent(Intent.ActionCall, uri);
                StartActivity(intent);
            }
            catch(Exception) // From (Exception e). James 04.11.16
            {
                //Log.Debug("o","pp");
                Finish();
            }
        }
    }
}