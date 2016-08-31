
using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Android.Support.Design.Widget;
using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Animation;
using Android.Views.Animations;
using DrawableCompat = Android.Support.V4.Graphics.Drawable.DrawableCompat;

namespace WCCMobile
{

    public class SwitchableFab : FloatingActionButton, ICheckable
    {
        AnimatorSet switchAnimation;
        bool state;
        Drawable srcFirst, srcSecond;
        ColorStateList backgroundTintFirst, backgroundTintSecond;
        /// <summary>
        /// The checked state attribute.
        /// </summary>
        static readonly int[] CheckedStateSet = {
            Android.Resource.Attribute.StateChecked
        };
        bool chked;
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchableFab"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SwitchableFab(Context context) :
            base(context)
        {
            Initialize(context, null);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchableFab"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="attrs">The attrs.</param>
        public SwitchableFab(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize(context, attrs);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchableFab"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="attrs">The attrs.</param>
        /// <param name="defStyle">The definition style.</param>
        public SwitchableFab(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(context, attrs);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchableFab"/> class.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="own">The jni ownership handle enumeration.</param>
        public SwitchableFab(IntPtr handle, JniHandleOwnership own)
            : base(handle, own)
        {
        }
        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="attrs">The attrs.</param>
        void Initialize(Context context, IAttributeSet attrs)
        {
            srcFirst = this.Drawable;
            backgroundTintFirst = this.BackgroundTintList;

            if (attrs == null)
                return;
            //var array = context.ObtainStyledAttributes(attrs, Resource.Styleable.SwitchableFab);
            //srcSecond = DrawableCompat.Wrap(array.GetDrawable(Resource.Styleable.SwitchableFab_srcSecond));
            DrawableCompat.SetTint(srcSecond, Android.Graphics.Color.White.ToArgb());
            //backgroundTintSecond = array.GetColorStateList(Resource.Styleable.SwitchableFab_backgroundTintSecond);
            //array.Recycle();
        }
        /// <summary>
        /// Call this view's OnClickListener, if it is defined.
        /// </summary>
        /// <returns>
        /// To be added.
        /// </returns>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Call this view's OnClickListener, if it is defined.  Performs all normal
        /// actions associated with clicking: reporting accessibility event, playing
        /// a sound, etc.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/View.html#performClick()" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        public override bool PerformClick()
        {
            Toggle();
            return base.PerformClick();
        }
        /// <summary>
        /// Change the checked state of the view to the inverse of its current state
        /// </summary>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Change the checked state of the view to the inverse of its current state
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/widget/Checkable.html#toggle()" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        public void Toggle()
        {
            JumpDrawablesToCurrentState();
            Checked = !Checked;
        }
        /// <summary>
        /// </summary>
        /// <value>
        /// To be added.
        /// </value>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc: Get method documentation">
        ///   <format type="text/html">
        ///     <b>Get method documentation</b>
        ///     <a href="http://developer.android.com/reference/android/widget/Checkable.html#isChecked()" target="_blank">[Android Documentation]</a>
        ///     <br />
        ///   </format>
        /// </para>
        /// <para tool="javadoc-to-mdoc: Set method documentation">
        ///   <format type="text/html">
        ///     <b>Set method documentation</b>
        ///     <a href="http://developer.android.com/reference/android/widget/Checkable.html#setChecked(boolean)" target="_blank">[Android Documentation]</a>
        ///     <br />
        ///   </format>Change the checked state of the view</para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        public bool Checked
        {
            get
            {
                return chked;
            }
            set
            {
                if (chked == value)
                    return;
                chked = value;
                RefreshDrawableState();
            }
        }
        /// <summary>
        /// Generate the new <c><see cref="T:Android.Graphics.Drawables.Drawable" /></c> state for
        /// this view.
        /// </summary>
        /// <param name="extraSpace">if non-zero, this is the number of extra entries you
        /// would like in the returned array in which you can place your own
        /// states.</param>
        /// <returns>
        /// To be added.
        /// </returns>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Generate the new <c><see cref="T:Android.Graphics.Drawables.Drawable" /></c> state for
        /// this view. This is called by the view
        /// system when the cached Drawable state is determined to be invalid.  To
        /// retrieve the current state, you should use <c><see cref="M:Android.Views.View.GetDrawableState" /></c>.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/widget/ImageView.html#onCreateDrawableState(int)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        public override int[] OnCreateDrawableState(int extraSpace)
        {
            var space = extraSpace + (Checked ? CheckedStateSet.Length : 0);
            var drawableState = base.OnCreateDrawableState(space);
            if (Checked)
                MergeDrawableStates(drawableState, CheckedStateSet);
            return drawableState;
        }
        /// <summary>
        /// Switches this instance.
        /// </summary>
        public void Switch()
        {
            if (state)
                Switch(srcFirst, backgroundTintFirst);
            else
                Switch(srcSecond, backgroundTintSecond);
            state = !state;
        }
        /// <summary>
        /// Switches the specified source.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="tint">The tint.</param>
        void Switch(Drawable src, ColorStateList tint)
        {
            const int ScaleDuration = 200;
            const int AlphaDuration = 150;
            const int AlphaInDelay = 50;
            const int InitialDelay = 100;

            if (switchAnimation != null)
            {
                switchAnimation.Cancel();
                switchAnimation = null;
            }

            var currentSrc = this.Drawable;

            // Scaling down animation
            var circleAnimOutX = ObjectAnimator.OfFloat(this, "scaleX", 1, 0.1f);
            var circleAnimOutY = ObjectAnimator.OfFloat(this, "scaleY", 1, 0.1f);
            circleAnimOutX.SetDuration(ScaleDuration);
            circleAnimOutY.SetDuration(ScaleDuration);

            // Alpha out of the icon
            var iconAnimOut = ObjectAnimator.OfInt(currentSrc, "alpha", 255, 0);
            iconAnimOut.SetDuration(AlphaDuration);

            var outSet = new AnimatorSet();
            outSet.PlayTogether(circleAnimOutX, circleAnimOutY, iconAnimOut);
            outSet.SetInterpolator(AnimationUtils.LoadInterpolator(Context,
                                                                     Android.Resource.Animation.AccelerateInterpolator));
            outSet.StartDelay = InitialDelay;
            outSet.AnimationEnd += (sender, e) =>
            {
                BackgroundTintList = tint;
                SetImageDrawable(src);
                JumpDrawablesToCurrentState();
                ((Animator)sender).RemoveAllListeners();
            };

            // Scaling up animation
            var circleAnimInX = ObjectAnimator.OfFloat(this, "scaleX", 0.1f, 1);
            var circleAnimInY = ObjectAnimator.OfFloat(this, "scaleY", 0.1f, 1);
            circleAnimInX.SetDuration(ScaleDuration);
            circleAnimInY.SetDuration(ScaleDuration);

            // Alpha in of the icon
            src.Alpha = 0;
            var iconAnimIn = ObjectAnimator.OfInt(src, "alpha", 0, 255);
            iconAnimIn.SetDuration(AlphaDuration);
            iconAnimIn.StartDelay = AlphaInDelay;

            var inSet = new AnimatorSet();
            inSet.PlayTogether(circleAnimInX, circleAnimInY, iconAnimIn);
            inSet.SetInterpolator(AnimationUtils.LoadInterpolator(Context,
                                                                    Android.Resource.Animation.DecelerateInterpolator));

            switchAnimation = new AnimatorSet();
            switchAnimation.PlaySequentially(outSet, inSet);
            switchAnimation.Start();
        }
    }
}