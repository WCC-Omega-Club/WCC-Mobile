
using Android.Views;
using Android.Widget;

namespace WCCMobile.Controls
{
	public class InfoBarController
	{
        /// <summary>
        /// The layout
        /// </summary>
        FrameLayout layout;
        /// <summary>
        /// The text
        /// </summary>
        TextView text;
        /// <summary>
        /// Initializes a new instance of the <see cref="InfoBarController"/> class.
        /// </summary>
        /// <param name="parentView">The parent view.</param>
        public InfoBarController (View parentView)
		{
			layout = parentView.FindViewById<FrameLayout> (Resource.Id.FlashBarLayout);
			text = parentView.FindViewById<TextView> (Resource.Id.FlashBarText);
		}
        /// <summary>
        /// Shows the loading dialouge.
        /// </summary>
        public void ShowLoading ()
		{
			text.Text = "Loading";
			layout.Visibility = ViewStates.Visible;
			layout.AlphaAnimate (1, duration: 400);
		}
        /// <summary>
        /// Shows the loaded dialouge and sets the dialouge to invisible.
        /// </summary>
        public void ShowLoaded ()
		{
			layout.AlphaAnimate (0, duration: 400, endAction: () => {
				layout.Visibility = ViewStates.Gone;
			});
		}
        /// <summary>
        /// Shows the information.
        /// </summary>
        /// <param name="info">The information.</param>
        public void ShowInformation (string info)
		{
			text.Text = info;
			layout.Visibility = ViewStates.Visible;
			layout.AlphaAnimate (1, duration: 500, endAction: () => {
				layout.AlphaAnimate (0, duration: 400, startDelay: 2000);
			});
		}
	}
}
