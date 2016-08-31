using Android.Content;
using Android.Views;
using Android.Widget;

namespace WCCMobile.Resources
{
    class ImageAdapter : BaseAdapter
    {
        Context context;
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageAdapter"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        public ImageAdapter(Context c)
        {
            context = c;
        }
        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public override int Count
        {
            get { return thumbIds.Length; }
        }
        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }
        /// <summary>
        /// Gets the item identifier.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public override long GetItemId(int position)
        {
            return 0;
        }

                
        /// <summary>
        /// Gets the view.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="convertView">The convert view.</param>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ImageView imageView;
            if (convertView == null) 
            {  // if it's not recycled, initialize some attributes
                imageView = new ImageView(context); //imageView.to
                //imageView.Touch += delegate { MainActivity.StartSubApp(position);};
                imageView.LayoutParameters = new AbsListView.LayoutParams(85*235/100, 85*475/100);//(position*size * 3 / 4, position* size * 3 / 4);//85,85
                imageView.SetScaleType(ImageView.ScaleType.CenterInside);
                imageView.SetPadding(8, 8, 8, 8); 
            }
            else
            {
                imageView = (ImageView)convertView; 
            }
            imageView.SetImageResource(thumbIds[position]);
            //imageView.SetImageBitmap(MainActivity.IMGSRC[3]);
            return imageView;
        }
        static int currentLabel = 0;
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public static int Label
        {
            set { currentLabel = value; }
            get { return  thumbIds[currentLabel]; }
        }
        //        
        /// <summary>
        /// The references to our images 
        /// </summary>
        static readonly int[] thumbIds = {
            Resource.Drawable.ic_map,
            Resource.Drawable.ic_directory,
            Resource.Drawable.ic_mail,
            Resource.Drawable.ic_blackboard,
            Resource.Drawable.ic_calendar,
            Resource.Drawable.ic_transit,
            Resource.Drawable.ic_library,
            Resource.Drawable.ic_athletics,
            Resource.Drawable.ic_career_services,
            Resource.Drawable.ic_counseling,
            Resource.Drawable.ic_dining_services,
            Resource.Drawable.ic_student_involvement,
            Resource.Drawable.ic_transfer_services,
            Resource.Drawable.placeholder_128x185,
            Resource.Drawable.placeholder_128x185,
            Resource.Drawable.placeholder_128x185,
            Resource.Drawable.placeholder_128x185,
            Resource.Drawable.placeholder_128x185
        };
    }
}