using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Runtime;
using Android.Views;

public static class AndroidExtensions
{
    static float density;
    static ColorDrawable defaultBgColor;
    /// <summary>
    /// Initializes the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    public static void Initialize(Context context)
    {
        var wm = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        var displayMetrics = new DisplayMetrics();
        wm.DefaultDisplay.GetMetrics(displayMetrics);
        density = displayMetrics.Density;

        var bg = new TypedValue();
        context.Theme.ResolveAttribute(Android.Resource.Attribute.ColorBackground, bg, true);
        defaultBgColor = new ColorDrawable(new Android.Graphics.Color(bg.Data));
    }
    /// <summary>
    /// Gets a value indicating whether this instance of the android OS supports material design.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is material; otherwise, <c>false</c>.
    /// </value>
    public static bool IsMaterial
    {
        get
        {
            return Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop;
        }
    }
    /// <summary>
    /// To the pixels.
    /// </summary>
    /// <param name="dp">The dp.</param>
    /// <returns></returns>
    public static int ToPixels(this int dp)
    {
        return (int)(dp * density + 0.5f);
    }
    /// <summary>
    /// Gets the default background.
    /// </summary>
    /// <value>
    /// The default background.
    /// </value>
    public static ColorDrawable DefaultBackground
    {
        get
        {
            return defaultBgColor;
        }
    }
}