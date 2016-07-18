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

namespace WCCMobile.Models
{
    public class ScheduleItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("s")]
        public string Street { get; set; }
        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("st")]
        public int StationType { get; set; }
        [JsonProperty("b")]
        public bool b { get; set; }
        [JsonProperty("su")]
        public bool su { get; set; }
        [JsonProperty("t")]
        public bool t { get; set; }
        [JsonProperty("bk")]
        public bool bk { get; set; }
        [JsonProperty("bl")]
        public bool bl { get; set; }
        [JsonProperty("la")]
        public double Latitude { get; set; }
        [JsonProperty("lo")]
        public double Longitude { get; set; }

        [JsonProperty("da")]
        public int EmptySlotCount { get; set; }
        [JsonProperty("dx")]
        public int dx { get; set; }
        [JsonProperty("ba")]
        public int BikeCount { get; set; }
        [JsonProperty("bx")]
        public int bx { get; set; }



        private Location location;
        [JsonIgnore]
        public Location Location
        {
            get
            {
                return location.Init ? location :
                    location = new Location { Lat = Latitude, Lon = Longitude, Init = true };
            }
        }

        [JsonIgnore]
        public int Capacity { get { return BikeCount + EmptySlotCount; } }




        [JsonIgnore]
        public bool Installed
        {
            get { return !Temporary; }
        }
        [JsonIgnore]
        public bool Temporary
        {
            get { return StationType == 2; }
        }
        [JsonIgnore]
        public bool Locked
        {
            get { return !Temporary && (b || (EmptySlotCount == 0 && BikeCount == 0)); }
        }

        public override bool Equals(object obj)
        {
            return obj is ScheduleItem && Equals((ScheduleItem)obj);
        }

        public bool Equals(ScheduleItem other)
        {
            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        [JsonIgnore]
        public string LocationUrl
        {
            get
            {
                var pos = Location.Lat + "," + Location.Lon;
                var location = "geo:" + pos + "?q=" + pos + "(" + Name.Replace(' ', '+') + ")";
                return location;
            }
        }
    }
}