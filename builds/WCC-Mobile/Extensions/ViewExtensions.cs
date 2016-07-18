

using Android.Views;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Views;
namespace WCCMobile {
    public static class ViewExtensions
    {
        /// <summary>
        /// Inflates and binds text view.
        /// </summary>
        /// <param name="parentView">The parent view.</param>
        /// <param name="textViewResourceId">The text view resource identifier.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static TextView InflateAndBindTextView(this View parentView, int textViewResourceId, string text)
        {
            TextView textView = null;

            if (parentView != null)
            {
                textView = parentView.FindViewById<TextView>(textViewResourceId);

                if (textView != null)
                {
                    textView.Text = text;
                }
            }

            return textView;
        }
        /// <summary>
        /// Inflates and binds edit text.
        /// </summary>
        /// <param name="parentView">The parent view.</param>
        /// <param name="textViewResourceId">The text view resource identifier.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static EditText InflateAndBindEditText(this View parentView, int textViewResourceId, string text)
        {
            EditText editText = null;

            if (parentView != null)
            {
                editText = parentView.FindViewById<EditText>(textViewResourceId);

                if (editText != null)
                {
                    editText.Text = text;
                }
            }

            return editText;
        }
        /// <summary>
        /// Inflates the and bind remote image view.
        /// </summary>
        /// <param name="parentView">The parent view.</param>
        /// <param name="imageViewResourceId">The image view resource identifier.</param>
        /// <param name="imageUrl">The image URL.</param>
        /// <returns></returns>
        public static ImageViewAsync InflateAndBindRemoteImageView(this View parentView, int imageViewResourceId, string imageUrl)
        {
            ImageViewAsync imageView = null;

            if (parentView != null)
            {
                imageView = parentView.FindViewById<ImageViewAsync>(imageViewResourceId);

                if (imageView != null)
                {
                //    ImageService.Instance.LoadUrl(imageUrl).Transform(new CircleTransformation()).Into(imageView);
                }
            }

            return imageView;
        }
        /// <summary>
        /// Inflates and binds local image view by resource.
        /// </summary>
        /// <param name="parentView">The parent view.</param>
        /// <param name="imageViewResourceId">The image view resource identifier.</param>
        /// <param name="resourceId">The resource identifier.</param>
        /// <returns></returns>
        public static ImageView InflateAndBindLocalImageViewByResource(this View parentView, int imageViewResourceId, int resourceId)
        {
            ImageView imageView = null;

            if (parentView != null)
            {
                imageView = parentView.FindViewById<ImageView>(imageViewResourceId);

                imageView.SetImageResource(resourceId);
            }

            return imageView;
        }
    }

}
