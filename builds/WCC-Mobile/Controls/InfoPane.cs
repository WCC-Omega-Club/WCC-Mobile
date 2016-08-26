using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Animation;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views.Animations;

namespace WCCMobile.Controls
{
    public class InfoPane : LinearLayout
    {
        public enum State
        {
            Closed,
            Opened,
            FullyOpened
        }

        public event Action<State> StateChanged;

        State state;
        int contentOffsetY;
        bool isAnimating;
        ITimeInterpolator smoothInterpolator = new SmoothInterpolator();
        ITimeInterpolator overInterpolator = new OvershootInterpolator();
        VelocityTracker velocityTracker;
        GestureDetector paneGestureDetector;
        State stateBeforeTracking;
        bool isTracking;
        bool preTracking;
        int startY = -1;
        float oldY;

        int pagingTouchSlop;
        int minFlingVelocity;
        int maxFlingVelocity;

        GradientDrawable shadowDrawable;
        /// <summary>
        /// Initializes a new instance of the <see cref="InfoPane"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public InfoPane(Context context) : base(context)
        {
            Initialize();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="InfoPane"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="attrs">The attrs.</param>
        public InfoPane(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="InfoPane"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="attrs">The attrs.</param>
        /// <param name="defStyle">The definition style.</param>
        public InfoPane(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Initialize();
        }
        /// <summary>
        /// Initializes the layout configuration of this instance.
        /// </summary>
        void Initialize()
        {
            var config = ViewConfiguration.Get(Context);
            this.pagingTouchSlop = config.ScaledPagingTouchSlop;
            this.minFlingVelocity = config.ScaledMinimumFlingVelocity;
            this.maxFlingVelocity = config.ScaledMaximumFlingVelocity;
            const int BaseShadowColor = 0;
            var shadowColors = new[] {
                Color.Argb (0x50, BaseShadowColor, BaseShadowColor, BaseShadowColor).ToArgb (),
                Color.Argb (0, BaseShadowColor, BaseShadowColor, BaseShadowColor).ToArgb ()
            };
            this.shadowDrawable = new GradientDrawable(GradientDrawable.Orientation.BottomTop,
                                                        shadowColors);
        }
        /// <summary>
        /// Gets a value indicating whether this <see cref="InfoPane"/> is opened.
        /// </summary>
        /// <value>
        ///   <c>true</c> if opened; otherwise, <c>false</c>.
        /// </value>
        public bool Opened
        {
            get
            {
                return state == State.Opened || state == State.FullyOpened;
            }
        }
        /// <summary>
        /// Gets a value indicating whether [fully opened].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [fully opened]; otherwise, <c>false</c>.
        /// </value>
        public bool FullyOpened
        {
            get
            {
                return state == State.FullyOpened;
            }
        }
        /// <summary>
        /// Sets the state of this instance of <see cref="InfoPane"/> offset status.
        /// </summary>
        /// <param name="newState">The new state.</param>
        /// <param name="animated">if set to <c>true</c> [animated].</param>
        public void SetState(State newState, bool animated = true)
        {
            var interpolator = state == State.Closed && newState == State.Opened ? overInterpolator : smoothInterpolator;
            this.state = newState;
            if (newState != State.Closed)
                Visibility = ViewStates.Visible;
            if (!animated)
            {
                SetNewOffset(OffsetForState(newState));
                if (state == State.Closed)
                    Visibility = ViewStates.Invisible;
            }
            else
            {
                isAnimating = true;
                var duration = Context.Resources.GetInteger(Android.Resource.Integer.ConfigMediumAnimTime);
                this.TranslationYAnimate(OffsetForState(newState), duration, interpolator, () => {
                    isAnimating = false;
                    if (state == State.Closed)
                        Visibility = ViewStates.Invisible;
                });
            }
            if (StateChanged != null)
                StateChanged(newState);
        }
        /// <summary>
        /// Changes the state of the <see cref="InfoPane"/> offset status.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        private int OffsetForState(State state)
        {
            switch (state)
            {
                default:
                case State.Closed:
                    return Height;
                case State.FullyOpened:
                    return 0;
                case State.Opened:
                    return Height - FindViewById(Resource.Id.PaneHeaderView).Height - PaddingTop;
            }
        }
        /// <summary>
        /// Sets a new offset for the infopane with respect to the global layout.
        /// </summary>
        /// <param name="newOffset">The new offset.</param>
        private void SetNewOffset(int newOffset)
        {
            if (state == State.Closed)
            {
                TranslationY = contentOffsetY = newOffset;
            }
            else
            {
                contentOffsetY = Math.Min(Math.Max(OffsetForState(State.FullyOpened), newOffset),
                                           OffsetForState(State.Opened));
                TranslationY = contentOffsetY;
            }
        }
        /// <summary>
        /// Called when [touch event].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (paneGestureDetector == null)
            {
                var l = new DoubleTapListener(() => SetState(Opened && FullyOpened ? State.Opened : State.FullyOpened));
                paneGestureDetector = new GestureDetector(Context, l);
            }
            paneGestureDetector.OnTouchEvent(e);

            e.OffsetLocation(0, TranslationY);
            if (e.Action == MotionEventActions.Down)
            {
                CaptureMovementCheck(e);
                return true;
            }
            if (!isTracking && !CaptureMovementCheck(e))
                return true;

            if (e.Action != MotionEventActions.Move || MoveDirectionTest(e))
                velocityTracker.AddMovement(e);

            if (e.Action == MotionEventActions.Move)
            {
                var y = e.GetY();
                // We don't want to go beyond startY
                if (state == State.Opened && y > startY
                    || state == State.FullyOpened && y < startY)
                    return true;
                // We reset the velocity tracker in case a movement goes back to its origin
                if (state == State.Opened && y > oldY
                    || state == State.FullyOpened && y < oldY)
                    velocityTracker.Clear();

                var traveledDistance = (int)Math.Round(Math.Abs(y - startY));
                if (state == State.Opened)
                    traveledDistance = OffsetForState(State.Opened) - traveledDistance;
                SetNewOffset(traveledDistance);
                oldY = y;
            }
            else if (e.Action == MotionEventActions.Up)
            {
                velocityTracker.ComputeCurrentVelocity(1000, maxFlingVelocity);
                if (Math.Abs(velocityTracker.YVelocity) > minFlingVelocity
                    && Math.Abs(velocityTracker.YVelocity) < maxFlingVelocity)
                    SetState(state == State.FullyOpened ? State.Opened : State.FullyOpened);
                else if (state == State.FullyOpened && contentOffsetY > Height / 2)
                    SetState(State.Opened);
                else if (state == State.Opened && contentOffsetY < Height / 2)
                    SetState(State.FullyOpened);
                else
                    SetState(state);

                preTracking = isTracking = false;
                velocityTracker.Clear();
                velocityTracker.Recycle();
            }

            return true;
        }
        /// <summary>
        /// Captures and performs the actions based on the args of <paramref name="ev"/>
        /// </summary>
        /// <param name="ev">The ev.</param>
        /// <returns></returns>
        bool CaptureMovementCheck(MotionEvent ev)
        {
            if (ev.Action == MotionEventActions.Down)
            {
                oldY = startY = (int)ev.GetY();

                if (!Opened)
                    return false;

                velocityTracker = VelocityTracker.Obtain();
                velocityTracker.AddMovement(ev);
                preTracking = true;
                stateBeforeTracking = state;
                return false;
            }

            if (ev.Action == MotionEventActions.Up)
                preTracking = isTracking = false;

            if (!preTracking)
                return false;

            velocityTracker.AddMovement(ev);

            if (ev.Action == MotionEventActions.Move)
            {

                // Check we are going in the right direction, if not cancel the current gesture
                if (!MoveDirectionTest(ev))
                {
                    preTracking = false;
                    return false;
                }

                // If the current gesture has not gone long enough don't intercept it just yet
                var distance = Math.Abs(ev.GetY() - startY);
                if (distance < pagingTouchSlop)
                    return false;
            }

            oldY = startY = (int)ev.GetY();
            isTracking = true;

            return true;
        }

           
        /// <summary>
        /// This method check's that movement is going in the right direction
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        bool MoveDirectionTest(MotionEvent e)
        {
            return (stateBeforeTracking == State.FullyOpened ? e.GetY() >= startY : e.GetY() <= startY);
        }
        /// <summary>
        /// Dispatches the drawer.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        protected override void DispatchDraw(Android.Graphics.Canvas canvas)
        {
            base.DispatchDraw(canvas);

            if (state == State.Opened || isTracking || isAnimating)
            {
                // Draw inset shadow on top of the pane
                shadowDrawable.SetBounds(0, 0, Width, PaddingTop);
                shadowDrawable.Draw(canvas);
            }
        }

        class SmoothInterpolator : Java.Lang.Object, ITimeInterpolator
        {
            /// <summary>
            /// Gets the interpolation.
            /// </summary>
            /// <param name="input">The input.</param>
            /// <returns></returns>
            public float GetInterpolation(float input)
            {
                return (float)Math.Pow(input - 1, 5) + 1;
            }
        }

        class DoubleTapListener : GestureDetector.SimpleOnGestureListener
        {
            Action callback;
            /// <summary>
            /// Initializes a new instance of the <see cref="DoubleTapListener"/> class.
            /// </summary>
            /// <param name="callback">The callback.</param>
            public DoubleTapListener(Action callback)
            {
                this.callback = callback;
            }
            /// <summary>
            /// Called when [double tap].
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns></returns>
            public override bool OnDoubleTap(MotionEvent e)
            {
                callback();
                return true;
            }
        }
    }
}
