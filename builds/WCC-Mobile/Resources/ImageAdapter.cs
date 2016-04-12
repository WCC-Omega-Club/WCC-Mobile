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

namespace WCCMobile.Resources
{
    class ImageAdapter : BaseAdapter
    {
        Context context;

        public ImageAdapter(Context c)
        {
            context = c;
        }

        public override int Count
        {
            get { return thumbIds.Length; }
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
            ImageView imageView;
            //Android.Util.Log.Debug("o",MainActivity.singleR.SubAppContainer.NumColumns.ToString());
            if (convertView == null)
            {  // if it's not recycled, initialize some attributes
                imageView = new ImageView(context);
                //int size = parent.Width / MainActivity.singleR.SubAppContainer.NumColumns;
                imageView.LayoutParameters = new AbsListView.LayoutParams(85*235/100, 85*475/100);//(position*size * 3 / 4, position* size * 3 / 4);//85,85
                imageView.SetScaleType(ImageView.ScaleType.CenterInside);
                imageView.SetPadding(8, 8, 8, 8);
            }
            else
            {
                imageView = (ImageView)convertView; 
            }
            imageView.SetImageResource(thumbIds[position]);
            return imageView;
        }

        // references to our images
        int[] thumbIds = { Resource.Drawable.calendar_143x155, Resource.Drawable.dining_services_115x169, Resource.Drawable.library_127x179, Resource.Drawable.mail_133x169
                         , Resource.Drawable.map_108x183, Resource.Drawable.placeholder_128x185, Resource.Drawable.placeholder_128x185, Resource.Drawable.placeholder_128x185
                         , Resource.Drawable.placeholder_128x185, Resource.Drawable.placeholder_128x185, Resource.Drawable.placeholder_128x185, Resource.Drawable.placeholder_128x185
                         , Resource.Drawable.placeholder_128x185, Resource.Drawable.placeholder_128x185, Resource.Drawable.placeholder_128x185, Resource.Drawable.placeholder_128x185 };
    }
}