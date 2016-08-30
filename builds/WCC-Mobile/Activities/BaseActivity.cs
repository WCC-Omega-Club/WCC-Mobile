using Android.OS;
using Android.Support.V7.Widget;
using Android.Support.V7.App;

namespace WCCMobile
{
    public abstract class BaseActivity : AppCompatActivity
    {
        /// <summary>
        /// Gets or sets the toolbar.
        /// </summary>
        /// <value>
        /// The toolbar.
        /// </value>
        public Toolbar Toolbar
        {
            get;
            set;
        }
        /// <summary>
        /// Called when [create].
        /// </summary>
        /// <param name="bundle">The bundle.</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(LayoutResource);
            Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            if (Toolbar != null)
            {
                SetSupportActionBar(Toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);

            }
        }
        /// <summary>
        /// Gets the layout resource.
        /// </summary>
        /// <value>
        /// The layout resource.
        /// </value>
        protected abstract int LayoutResource
        {
            get;
        }
        /// <summary>
        /// Sets the action bar icon.
        /// </summary>
        /// <value>
        /// The action bar icon.
        /// </value>
        protected int ActionBarIcon
        {
            set { Toolbar.SetNavigationIcon(value); }
        }
    }
}