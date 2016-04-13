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
        private static readonly string empty_email_guard = "No email available";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.PhoneBookLayout);
            ActionBar.SetIcon(ImageAdapter.Label);
            PhoneBookList = (ListView)FindViewById(Resource.Id.PhoneBookList);
            PhoneBookList.Adapter = new YellowBookAdapter(this);
            PhoneBookList.ItemLongClick += SendEmail;
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
        /// <summary>
        /// Gets the clicked TextView and calls a substring of the .Text property - substring is 
        /// the rest of the string after the return or newline character offset by +10.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void SendEmail(object sender, AdapterView.ItemLongClickEventArgs args)
        {

            SendEmail(args.Position, this);

        }

        /// <summary>
        /// Method to send emails to address listed in YellowBook.txt - Any Activity can call this
        /// method provided they pass themselves and the key to the address in the book
        /// </summary>
        /// <param name="EmailKey"></param>
        /// <param name="Mailer"></param>
        public static void SendEmail(int EmailKey, Activity Mailer)
        {
            try
            {
                string p2Email;
                if (!YellowBookAdapter.getBook.TryGetValue(EmailKey, out p2Email)) return;
                Android.Util.Log.Debug("Email Function", "Successfully Found :" + p2Email);
                int pos = 0;
                while (p2Email[pos] != '\r' && p2Email[pos] != '\n') ++pos;
                if (p2Email.Substring(pos + 12) != empty_email_guard)
                {
                    var intent = new Intent(Intent.ActionSend);
                    Log.Debug("Email", p2Email.Substring(pos + 12));
                    intent.PutExtra(Intent.ExtraEmail, new string[] { p2Email.Substring(pos + 12) });
                    intent.SetType("message/rfc822");
                    Mailer.StartActivity(intent);
                }
            }
            catch (Exception e)
            { }
        }
    }
}