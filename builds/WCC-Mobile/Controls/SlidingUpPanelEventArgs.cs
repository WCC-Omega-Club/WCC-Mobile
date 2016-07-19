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

namespace WCCMobile
{
    public delegate void SlidingUpPanelEventHandler(object sender, SlidingUpPanelEventArgs args);
    public delegate void SlidingUpPanelSlideEventHandler(object sender, SlidingUpPanelSlideEventArgs args);

    public class SlidingUpPanelEventArgs : EventArgs
    {
        public View Panel { get; set; }
    }

    public class SlidingUpPanelSlideEventArgs : SlidingUpPanelEventArgs
    {
        public float SlideOffset { get; set; }
    }
}