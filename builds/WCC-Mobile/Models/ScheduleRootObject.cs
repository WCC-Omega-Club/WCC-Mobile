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
using Newtonsoft.Json;
using WCCMobile.Models;

namespace WCCMobile
{
    public class ScheduleRootObject
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        [JsonProperty("schemeSuspended")]
        public bool SchemeSuspended { get; set; }
        [JsonProperty("schedule")]
        public List<ScheduleItem> Schedule { get; set; }
    }
}