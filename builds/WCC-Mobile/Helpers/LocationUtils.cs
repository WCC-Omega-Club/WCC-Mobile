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
using System.IO;
using Android.Gms.Maps.Model;

namespace WCCMobile
{
    public class LocationUtils
    {
        /// <summary>
        /// Calculates the geodesic between <paramref name="p1"/> and <paramref name="p2"/> using law of cosines.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns></returns>
        public static double Distance(Location p1, Location p2)
        {
            const int EarthRadius = 6371;
            return Math.Acos(Math.Sin(DegToRad(p1.Lat)) * Math.Sin(DegToRad(p2.Lat)) +
                              Math.Cos(DegToRad(p1.Lat)) * Math.Cos(DegToRad(p2.Lat)) *
                              Math.Cos(DegToRad(p2.Lon) - DegToRad(p1.Lon))) * EarthRadius;
        }

        public static double Distance(LatLng p1, LatLng p2)
        {
            const int EarthRadius = 6371;
            return Math.Acos(Math.Sin(DegToRad(p1.Latitude)) * Math.Sin(DegToRad(p2.Latitude)) +
                              Math.Cos(DegToRad(p1.Latitude)) * Math.Cos(DegToRad(p2.Latitude)) *
                              Math.Cos(DegToRad(p2.Longitude) - DegToRad(p1.Longitude))) * EarthRadius;
        } 
        /// <summary>
        /// Converts Degrees to Radians.
        /// </summary>
        /// <param name="deg">The deg.</param>
        /// <returns></returns>
        static double DegToRad(double deg)
        {
            return deg * Math.PI / 180;
        }

    }
}