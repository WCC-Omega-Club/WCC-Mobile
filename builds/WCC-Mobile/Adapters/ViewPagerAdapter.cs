using System;
using System.Collections.Generic;
using Android.Gms.Maps;
using JavaString = Java.Lang.String;
using Android.Support.V4.App;

namespace WCCMobile.Adapters
{
    public class ViewPagerAdapter : FragmentPagerAdapter
    {
        List<Fragment> fragments;
        /// <summary>
        /// The titles
        /// </summary>
        public static JavaString[] Titles = new[]
        {
            new JavaString("Campus Map"),
            new JavaString("WCC Mobile Home"),
        };
        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public override int Count
        {
            get { return fragments.Count; }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewPagerAdapter"/> class.
        /// </summary>
        /// <param name="fm">The fm.</param>
        public ViewPagerAdapter(FragmentManager fm) : base(fm)
        {
            this.fragments = new List<Fragment>();

            fragments.Add(new SupportMapFragment());
            fragments.Add(new Fragment());
            //fragments.Add(new MyMapFragment());
            //fragments.Add(new Fragment2());
            //fragments.Add(new Fragment3());

        }
        /// <summary>
        /// Gets the page title formatted.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            return Titles[position];
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public override Fragment GetItem(int position)
        {
            return fragments[position];
        }


    }
}

