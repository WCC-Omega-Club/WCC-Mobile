using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using Android.Views.Accessibility;
using Java.Interop;

namespace WCCMobile
{
    public class SlidingUpPanelLayout : ViewGroup
    {
        #region Default Fields
        private new readonly string Tag = "SlidingUpPanelLayout";
        private const int DefaultPanelHeight = 68;
        private const int DefaultShadowHeight = 4;
        private const int DefaultMinFlingVelocity = 400;
        private const bool DefaultOverlayFlag = false;
        private static readonly Color DefaultFadeColor = new Color(0, 0, 0, 99);
        private static readonly int[] DefaultAttrs = { Android.Resource.Attribute.Gravity };
        #endregion
        
        #region Fields
        private readonly int minFlingVelocity = DefaultMinFlingVelocity;
        private Color coveredFadeColor = DefaultFadeColor;
        private readonly Paint coveredFadePaint = new Paint();
        private int panelHeight = -1;
        private readonly int shadowHeight = -1;
        private readonly bool isSlidingUp;
        private bool canSlide;
        private View dragView;
        private View slideableView;
        private readonly int dragViewResId = -1;
        private SlideState slideState = SlideState.Collapsed;
        private float slideOffset;
        private int slideRange;
        private bool isUnableToDrag;
        private readonly int scrollTouchSlop;
        private float initialMotionX;
        private float initialMotionY;
        private float anchorPoint;
        private readonly ViewDragHelper dragHelper;
        private bool firstLayout = true;
        private readonly Rect tmpRect = new Rect();
        #endregion
        
        #region Event Handlers
        public event SlidingUpPanelSlideEventHandler PanelSlide;
        public event SlidingUpPanelEventHandler PanelCollapsed;
        public event SlidingUpPanelEventHandler PanelExpanded;
        public event SlidingUpPanelEventHandler PanelAnchored;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpanded
        {
            get { return slideState == SlideState.Expanded; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is anchored.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is anchored; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnchored
        {
            get { return slideState == SlideState.Anchored; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is slideable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is slideable; otherwise, <c>false</c>.
        /// </value>
        public bool IsSlideable
        {
            get { return canSlide; }
        }
        /// <summary>
        /// Gets or sets the color to fade to.
        /// </summary>
        /// <value>
        /// The color of the covered fade.
        /// </value>
        public Color CoveredFadeColor
        {
            get { return coveredFadeColor; }
            set
            {
                coveredFadeColor = value;
                Invalidate();
            }
        }
        /// <summary>
        /// Gets or sets the height of the panel.
        /// </summary>
        /// <value>
        /// The height of the panel.
        /// </value>
        public int PanelHeight
        {
            get { return panelHeight; }
            set
            {
                panelHeight = value;
                RequestLayout();
            }
        }
        /// <summary>
        /// Gets or sets the drag view.
        /// </summary>
        /// <value>
        /// The drag view.
        /// </value>
         public View DragView
        {
            get { return dragView; }
            set { dragView = value; }
        }
        /// <summary>
        /// Gets or sets the anchor point.
        /// </summary>
        /// <value>
        /// The anchor point.
        /// </value>
        public float AnchorPoint
        {
            get { return anchorPoint; }
            set
            {
                if (value > 0 && value < 1)
                    anchorPoint = value;
            }
        }
        /// <summary>
        /// Gets or sets the shadow drawable.
        /// </summary>
        /// <value>
        /// The shadow drawable.
        /// </value>
        public Drawable ShadowDrawable { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [sliding enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sliding enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool SlidingEnabled { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [overlay content].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [overlay content]; otherwise, <c>false</c>.
        /// </value>
        public bool OverlayContent { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is using drag view touch events.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is using drag view touch events; otherwise, <c>false</c>.
        /// </value>
        public bool IsUsingDragViewTouchEvents { get; set; }
        /// <summary>
        /// Gets the value indication whether [top sliding].
        /// </summary>
        /// <value>
        /// The sliding top.
        /// </value>
        private int SlidingTop
        {
            get
            {
                if (slideableView != null)
                {
                    return isSlidingUp
                        ? MeasuredHeight - PaddingBottom - slideableView.MeasuredHeight
                        : MeasuredHeight - PaddingBottom - (slideableView.MeasuredHeight * 2);
                }

                return MeasuredHeight - PaddingBottom;
            }
        }
        /// <summary>
        /// Gets a value indicating whether [pane visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [pane visible]; otherwise, <c>false</c>.
        /// </value>
        public bool PaneVisible
        {
            get
            {
                if (ChildCount < 2)
                    return false;
                var slidingPane = GetChildAt(1);
                return slidingPane.Visibility == ViewStates.Visible;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SlidingUpPanelLayout"/> class.
        /// </summary>
        /// <param name="javaReference">The java reference.</param>
        /// <param name="transfer">The transfer.</param>
        public SlidingUpPanelLayout(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SlidingUpPanelLayout"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SlidingUpPanelLayout(Context context)
            : this(context, null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SlidingUpPanelLayout"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="attrs">The attrs.</param>
        public SlidingUpPanelLayout(Context context, IAttributeSet attrs)
            : this(context, attrs, 0) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SlidingUpPanelLayout"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="attrs">The attrs.</param>
        /// <param name="defStyle">The definition style.</param>
        /// <exception cref="ArgumentException">gravity must be set to either top or bottom</exception>
        public SlidingUpPanelLayout(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            // not really relevan in Xamarin.Android but keeping for a possible
            // future update which will render layouts in the Designer.
            if (IsInEditMode) return;

            if (attrs != null)
            {
                var defAttrs = context.ObtainStyledAttributes(attrs, DefaultAttrs);

                if (defAttrs.Length() > 0)
                {
                    var gravity = defAttrs.GetInt(0, (int)GravityFlags.NoGravity);
                    var gravityFlag = (GravityFlags)gravity;
                    if (gravityFlag != GravityFlags.Top && gravityFlag != GravityFlags.Bottom)
                        throw new ArgumentException("gravity must be set to either top or bottom");
                    isSlidingUp = gravityFlag == GravityFlags.Bottom;
                }

                defAttrs.Recycle();

                var ta = context.ObtainStyledAttributes(attrs, Resource.Styleable.SlidingUpPanelLayout);

                if (ta.Length() > 0)
                {
                    panelHeight = ta.GetDimensionPixelSize(Resource.Styleable.SlidingUpPanelLayout_collapsedHeight, -1);
                    shadowHeight = ta.GetDimensionPixelSize(Resource.Styleable.SlidingUpPanelLayout_shadowHeight, -1);

                    minFlingVelocity = ta.GetInt(Resource.Styleable.SlidingUpPanelLayout_flingVelocity,
                        DefaultMinFlingVelocity);
                    coveredFadeColor = ta.GetColor(Resource.Styleable.SlidingUpPanelLayout_fadeColor, DefaultFadeColor);

                    dragViewResId = ta.GetResourceId(Resource.Styleable.SlidingUpPanelLayout_dragView, -1);

                    OverlayContent = ta.GetBoolean(Resource.Styleable.SlidingUpPanelLayout_overlay, DefaultOverlayFlag);
                }

                ta.Recycle();
            }

            var density = context.Resources.DisplayMetrics.Density;
            if (panelHeight == -1)
                panelHeight = (int)(DefaultPanelHeight * density + 0.5f);
            if (shadowHeight == -1)
                shadowHeight = (int)(DefaultShadowHeight * density + 0.5f);

            SetWillNotDraw(false);

            dragHelper = ViewDragHelper.Create(this, 0.5f, new DragHelperCallback(this));
            dragHelper.MinVelocity = minFlingVelocity * density;

            canSlide = true;
            SlidingEnabled = true;

            var vc = ViewConfiguration.Get(context);
            scrollTouchSlop = vc.ScaledTouchSlop;
        }
        #endregion

        #region Event Methods
        /// <summary>
        /// Finalize inflating a view from XML.
        /// </summary>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Finalize inflating a view from XML.  This is called as the last phase
        /// of inflation, after all child views have been added.
        /// </para>
        /// <para tool="javadoc-to-mdoc">Even if the subclass overrides onFinishInflate, they should always be
        /// sure to call the super method, so that we get called.
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/View.html#onFinishInflate()" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            if (dragViewResId != -1)
                dragView = FindViewById(dragViewResId);
        }
        /// <summary>
        /// Called when [panel slide].
        /// </summary>
        /// <param name="panel">The panel.</param>
        private void OnPanelSlide(View panel)
        {
            if (PanelSlide != null)
                PanelSlide(this, new SlidingUpPanelSlideEventArgs { Panel = panel, SlideOffset = slideOffset });
        }
        /// <summary>
        /// Called when [panel collapsed].
        /// </summary>
        /// <param name="panel">The panel.</param>
        private void OnPanelCollapsed(View panel)
        {
            if (PanelCollapsed != null)
                PanelCollapsed(this, new SlidingUpPanelEventArgs { Panel = panel });
            SendAccessibilityEvent(EventTypes.WindowStateChanged);
        }
        /// <summary>
        /// Called when [panel anchored].
        /// </summary>
        /// <param name="panel">The panel.</param>
        private void OnPanelAnchored(View panel)
        {
            if (PanelAnchored != null)
                PanelAnchored(this, new SlidingUpPanelEventArgs { Panel = panel });
            SendAccessibilityEvent(EventTypes.WindowStateChanged);
        }
        /// <summary>
        /// Called when [panel expanded].
        /// </summary>
        /// <param name="panel">The panel.</param>
        private void OnPanelExpanded(View panel)
        {
            if (PanelExpanded != null)
                PanelExpanded(this, new SlidingUpPanelEventArgs { Panel = panel });
            SendAccessibilityEvent(EventTypes.WindowStateChanged);
        }
        /// <summary>
        /// Called when [panel dragged].
        /// </summary>
        /// <param name="newTop">The new top.</param>
        private void OnPanelDragged(int newTop)
        {
            slideOffset = isSlidingUp
                ? (float)(newTop - SlidingTop) / slideRange
                : (float)(SlidingTop - newTop) / slideRange;
            OnPanelSlide(slideableView);
        }
        /// <summary>
        /// This is called when the view is attached to a window.
        /// </summary>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">This is called when the view is attached to a window.  At this point it
        /// has a Surface and will start drawing.  Note that this function is
        /// guaranteed to be called before <c><see cref="M:Android.Views.View.OnDraw(Android.Graphics.Canvas)" /></c>,
        /// however it may be called any time before the first onDraw -- including
        /// before or after <c><see cref="M:Android.Views.View.OnMeasure(System.Int32, System.Int32)" /></c>.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/View.html#onAttachedToWindow()" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        /// <altmember cref="M:Android.Views.View.OnDetachedFromWindow" />
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            firstLayout = true;
        }
        /// <summary>
        /// This is called when the view is detached from a window.
        /// </summary>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">This is called when the view is detached from a window.  At this point it
        /// no longer has a surface for drawing.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/View.html#onDetachedFromWindow()" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        /// <altmember cref="M:Android.Views.View.OnAttachedToWindow" />
        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            firstLayout = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="widthMeasureSpec">horizontal space requirements as imposed by the parent.
        /// The requirements are encoded with
        /// <c><see cref="T:Android.Views.View+MeasureSpec" /></c>.</param>
        /// <param name="heightMeasureSpec">vertical space requirements as imposed by the parent.
        /// The requirements are encoded with
        /// <c><see cref="T:Android.Views.View+MeasureSpec" /></c>.</param>
        /// <exception cref="InvalidOperationException">Width must have an exact value or match_parent
        /// or
        /// Height must have an exact value or match_parent</exception>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc" />
        /// <para tool="javadoc-to-mdoc">
        /// Measure the view and its content to determine the measured width and the
        /// measured height. This method is invoked by <c><see cref="M:Android.Views.View.Measure(System.Int32, System.Int32)" /></c> and
        /// should be overriden by subclasses to provide accurate and efficient
        /// measurement of their contents.
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <i>CONTRACT:</i> When overriding this method, you
        /// <i>must</i> call <c><see cref="M:Android.Views.View.SetMeasuredDimension(System.Int32, System.Int32)" /></c> to store the
        /// measured width and height of this view. Failure to do so will trigger an
        /// <c>IllegalStateException</c>, thrown by
        /// <c><see cref="M:Android.Views.View.Measure(System.Int32, System.Int32)" /></c>. Calling the superclass'
        /// <c><see cref="M:Android.Views.View.OnMeasure(System.Int32, System.Int32)" /></c> is a valid use.
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        /// The base class implementation of measure defaults to the background size,
        /// unless a larger size is allowed by the MeasureSpec. Subclasses should
        /// override <c><see cref="M:Android.Views.View.OnMeasure(System.Int32, System.Int32)" /></c> to provide better measurements of
        /// their content.
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        /// If this method is overridden, it is the subclass's responsibility to make
        /// sure the measured height and width are at least the view's minimum height
        /// and width (<c><see cref="M:Android.Views.View.get_SuggestedMinimumHeight" /></c> and
        /// <c><see cref="M:Android.Views.View.get_SuggestedMinimumWidth" /></c>).
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/View.html#onMeasure(int, int)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        /// <altmember cref="P:Android.Views.View.MeasuredWidth" />
        /// <altmember cref="P:Android.Views.View.MeasuredHeight" />
        /// <altmember cref="M:Android.Views.View.SetMeasuredDimension(System.Int32, System.Int32)" />
        /// <altmember cref="M:Android.Views.View.get_SuggestedMinimumHeight" />
        /// <altmember cref="M:Android.Views.View.get_SuggestedMinimumWidth" />
        /// <altmember cref="M:Android.Views.View.MeasureSpec.GetMode(System.Int32)" />
        /// <altmember cref="M:Android.Views.View.MeasureSpec.GetSize(System.Int32)" />
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            var widthSize = MeasureSpec.GetSize(widthMeasureSpec);
            var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
            var heightSize = MeasureSpec.GetSize(heightMeasureSpec);

            if (widthMode != MeasureSpecMode.Exactly)
                throw new InvalidOperationException("Width must have an exact value or match_parent");
            if (heightMode != MeasureSpecMode.Exactly)
                throw new InvalidOperationException("Height must have an exact value or match_parent");

            var layoutHeight = heightSize - PaddingTop - PaddingBottom;
            var panelHeight = this.panelHeight;

            if (ChildCount > 2)
                Log.Error(Tag, "OnMeasure: More than two child views are not supported.");
            else
                panelHeight = 0;

            slideableView = null;
            canSlide = false;

            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                var lp = (LayoutParams)child.LayoutParameters;

                var height = layoutHeight;
                if (child.Visibility == ViewStates.Gone)
                {
                    lp.DimWhenOffset = false;
                    continue;
                }

                if (i == 1)
                {
                    lp.Slideable = true;
                    lp.DimWhenOffset = true;
                    slideableView = child;
                    canSlide = true;
                }
                else
                {
                    if (!OverlayContent)
                        height -= panelHeight;
                }

                int childWidthSpec;
                if (lp.Width == ViewGroup.LayoutParams.WrapContent)
                    childWidthSpec = MeasureSpec.MakeMeasureSpec(widthSize, MeasureSpecMode.AtMost);
                else if (lp.Width == ViewGroup.LayoutParams.MatchParent)
                    childWidthSpec = MeasureSpec.MakeMeasureSpec(widthSize, MeasureSpecMode.Exactly);
                else
                    childWidthSpec = MeasureSpec.MakeMeasureSpec(lp.Width, MeasureSpecMode.Exactly);

                int childHeightSpec;
                if (lp.Height == ViewGroup.LayoutParams.WrapContent)
                    childHeightSpec = MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.AtMost);
                else if (lp.Height == ViewGroup.LayoutParams.MatchParent)
                    childHeightSpec = MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly);
                else
                    childHeightSpec = MeasureSpec.MakeMeasureSpec(lp.Height, MeasureSpecMode.Exactly);

                child.Measure(childWidthSpec, childHeightSpec);
            }
            SetMeasuredDimension(widthSize, heightSize);
        }
        /// <summary>
        /// Called from layout when this view should
        /// assign a size and position to each of its children.
        /// </summary>
        /// <param name="changed">This is a new size or position for this view</param>
        /// <param name="l">Left position, relative to parent</param>
        /// <param name="t">Top position, relative to parent</param>
        /// <param name="r">Right position, relative to parent</param>
        /// <param name="b">Bottom position, relative to parent</param>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Called from layout when this view should
        /// assign a size and position to each of its children.
        /// Derived classes with children should override
        /// this method and call layout on each of
        /// their children.
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/ViewGroup.html#onLayout(boolean, int, int, int, int)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            if (firstLayout)
            {
                switch (slideState)
                {
                    case SlideState.Expanded:
                        slideOffset = canSlide ? 0.0f : 1.0f;
                        break;
                    case SlideState.Anchored:
                        slideOffset = canSlide ? anchorPoint : 1.0f;
                        break;
                    case SlideState.Collapsed:
                        slideOffset = 1.0f;
                        break;
                }
            }

            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);

                if (child.Visibility == ViewStates.Gone)
                    continue;

                var lp = (LayoutParams)child.LayoutParameters;
                var childHeight = child.MeasuredHeight;

                if (lp.Slideable)
                    slideRange = childHeight - panelHeight;

                int childTop;
                if (isSlidingUp)
                    childTop = lp.Slideable ? SlidingTop + (int)(slideRange * slideOffset) : PaddingTop;
                else
                    childTop = lp.Slideable ? SlidingTop - (int)(slideRange * slideOffset) : PaddingTop + PanelHeight;

                var childBottom = childTop + childHeight;
                var childLeft = PaddingLeft;
                var childRight = childLeft + child.MeasuredWidth;

                child.Layout(childLeft, childTop, childRight, childBottom);
            }

            if (firstLayout)
                UpdateObscuredViewVisibility();

            firstLayout = false;
        }
        /// <summary>
        /// This is called during layout when the size of this view has changed.
        /// </summary>
        /// <param name="w">Current width of this view.</param>
        /// <param name="h">Current height of this view.</param>
        /// <param name="oldw">Old width of this view.</param>
        /// <param name="oldh">Old height of this view.</param>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">This is called during layout when the size of this view has changed. If
        /// you were just added to the view hierarchy, you're called with the old
        /// values of 0.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/View.html#onSizeChanged(int, int, int, int)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            if (h != oldh)
                firstLayout = true;
        }
        /// <summary>
        /// Implement this method to intercept all touch screen motion events.
        /// </summary>
        /// <param name="ev">The motion event being dispatched down the hierarchy.</param>
        /// <returns>
        /// To be added.
        /// </returns>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Implement this method to intercept all touch screen motion events.  This
        /// allows you to watch events as they are dispatched to your children, and
        /// take ownership of the current gesture at any point.
        /// </para>
        /// <para tool="javadoc-to-mdoc">Using this function takes some care, as it has a fairly complicated
        /// interaction with <c><see cref="M:Android.Views.View.OnTouchEvent(Android.Views.MotionEvent)" /></c>, and using it requires implementing
        /// that method as well as this one in the correct way.  Events will be
        /// received in the following order:
        /// <list type="number"><item><term> You will receive the down event here.
        /// </term></item><item><term> The down event will be handled either by a child of this view
        /// group, or given to your own onTouchEvent() method to handle; this means
        /// you should implement onTouchEvent() to return true, so you will
        /// continue to see the rest of the gesture (instead of looking for
        /// a parent view to handle it).  Also, by returning true from
        /// onTouchEvent(), you will not receive any following
        /// events in onInterceptTouchEvent() and all touch processing must
        /// happen in onTouchEvent() like normal.
        /// </term></item><item><term> For as long as you return false from this function, each following
        /// event (up to and including the final up) will be delivered first here
        /// and then to the target's onTouchEvent().
        /// </term></item><item><term> If you return true from here, you will not receive any
        /// following events: the target view will receive the same event but
        /// with the action <c><see cref="F:Android.Views.MotionEventActions.Cancel" /></c>, and all further
        /// events will be delivered to your onTouchEvent() method and no longer
        /// appear here.
        /// </term></item></list></para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/ViewGroup.html#onInterceptTouchEvent(android.view.MotionEvent)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            var action = MotionEventCompat.GetActionMasked(ev);

            if (!canSlide || !SlidingEnabled || (isUnableToDrag && action != (int)MotionEventActions.Down))
            {
                dragHelper.Cancel();
                return base.OnInterceptTouchEvent(ev);
            }

            if (action == (int)MotionEventActions.Cancel || action == (int)MotionEventActions.Up)
            {
                dragHelper.Cancel();
                return false;
            }

            var x = ev.GetX();
            var y = ev.GetY();
            var interceptTap = false;

            switch (action)
            {
                case (int)MotionEventActions.Down:
                    isUnableToDrag = false;
                    initialMotionX = x;
                    initialMotionY = y;
                    if (IsDragViewUnder((int)x, (int)y) && !IsUsingDragViewTouchEvents)
                        interceptTap = true;
                    break;
                case (int)MotionEventActions.Move:
                    var adx = Math.Abs(x - initialMotionX);
                    var ady = Math.Abs(y - initialMotionY);
                    var dragSlop = dragHelper.TouchSlop;

                    if (IsUsingDragViewTouchEvents)
                    {
                        if (adx > scrollTouchSlop && ady < scrollTouchSlop)
                            return base.OnInterceptTouchEvent(ev);
                        if (ady > scrollTouchSlop)
                            interceptTap = IsDragViewUnder((int)x, (int)y);
                    }

                    if ((ady > dragSlop && adx > ady) || !IsDragViewUnder((int)x, (int)y))
                    {
                        dragHelper.Cancel();
                        isUnableToDrag = true;
                        return false;
                    }
                    break;
            }

            var interceptForDrag = dragHelper.ShouldInterceptTouchEvent(ev);

            return interceptForDrag || interceptTap;
        }
        /// <summary>
        /// Called when [touch event].
        /// </summary>
        /// <param name="ev">The ev.</param>
        /// <returns></returns>
        public override bool OnTouchEvent(MotionEvent ev)
        {
            if (!canSlide || !SlidingEnabled)
                return base.OnTouchEvent(ev);

            dragHelper.ProcessTouchEvent(ev);
            var action = (int)ev.Action;

            switch (action & MotionEventCompat.ActionMask)
            {
                case (int)MotionEventActions.Down:
                    {
                        var x = ev.GetX();
                        var y = ev.GetY();
                        initialMotionX = x;
                        initialMotionY = y;
                        break;
                    }
                case (int)MotionEventActions.Up:
                    {
                        var x = ev.GetX();
                        var y = ev.GetY();
                        var dx = x - initialMotionX;
                        var dy = y - initialMotionY;
                        var slop = dragHelper.TouchSlop;
                        var dragView = this.dragView ?? slideableView;
                        if (dx * dx + dy * dy < slop * slop && IsDragViewUnder((int)x, (int)y))
                        {
                            dragView.PlaySoundEffect(SoundEffects.Click);
                            if (!IsExpanded && !IsAnchored)
                                ExpandPane(anchorPoint);
                            else
                                CollapsePane();
                        }
                        break;
                    }
            }

            return true;
        }
        #endregion

        /// <summary>
        /// Updates the obscured view to be more or less visible.
        /// </summary>
        private void UpdateObscuredViewVisibility()
        {
            if (ChildCount == 0) return;

            var leftBound = PaddingLeft;
            var rightBound = Width - PaddingLeft;
            var topBound = PaddingTop;
            var bottomBound = Height - PaddingBottom;
            int left;
            int right;
            int top;
            int bottom;

            if (slideableView != null && HasOpaqueBackground(slideableView))
            {
                left = slideableView.Left;
                right = slideableView.Right;
                top = slideableView.Top;
                bottom = slideableView.Bottom;
            }
            else
                left = right = top = bottom = 0;

            var child = GetChildAt(0);
            var clampedChildLeft = Math.Max(leftBound, child.Left);
            var clampedChildTop = Math.Max(topBound, child.Top);
            var clampedChildRight = Math.Max(rightBound, child.Right);
            var clampedChildBottom = Math.Max(bottomBound, child.Bottom);
            ViewStates vis;
            if (clampedChildLeft >= left && clampedChildTop >= top &&
                clampedChildRight <= right && clampedChildBottom <= bottom)
                vis = ViewStates.Invisible;
            else
                vis = ViewStates.Visible;
            child.Visibility = vis;
        }
        /// <summary>
        /// Sets all children visible.
        /// </summary>
        private void SetAllChildrenVisible()
        {
            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (child.Visibility == ViewStates.Invisible)
                    child.Visibility = ViewStates.Visible;
            }
        }
        /// <summary>
        /// Determines whether [has opaque background] [the specified view].
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns></returns>
        private static bool HasOpaqueBackground(View view)
        {
            var bg = view.Background;
            if (bg != null)
                return bg.Opacity == (int)Format.Opaque;
            return false;
        }
         
        /// <summary>
        /// Determines whether [is drag view under] [the specified x].
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        private bool IsDragViewUnder(int x, int y)
        {
            var dragView = this.dragView ?? slideableView;
            if (dragView == null) return false;

            var viewLocation = new int[2];
            dragView.GetLocationOnScreen(viewLocation);
            var parentLocation = new int[2];
            GetLocationOnScreen(parentLocation);

            var screenX = parentLocation[0] + x;
            var screenY = parentLocation[1] + y;
            return screenX >= viewLocation[0] && screenX < viewLocation[0] + dragView.Width &&
                   screenY >= viewLocation[1] && screenY < viewLocation[1] + dragView.Height;
        }
        /// <summary>
        /// Collapses the pane.
        /// </summary>
        /// <returns></returns>
        public bool CollapsePane()
        {
            if (firstLayout || SmoothSlideTo(1.0f))
                return true;
            return false;
        }
        /// <summary>
        /// Expands the pane.
        /// </summary>
        /// <returns></returns>
        public bool ExpandPane()
        {
            return ExpandPane(0);
        }
        /// <summary>
        /// Expands the pane.
        /// </summary>
        /// <param name="slideOffset">The slide offset.</param>
        /// <returns></returns>
        public bool ExpandPane(float slideOffset)
        {
            if (!PaneVisible)
                ShowPane();
            return firstLayout || SmoothSlideTo(slideOffset);
        }
        /// <summary>
        /// Shows the pane.
        /// </summary>
        public void ShowPane()
        {
            if (ChildCount < 2) return;

            var slidingPane = GetChildAt(1);
            slidingPane.Visibility = ViewStates.Visible;
            RequestLayout();
        }
        /// <summary>
        /// Hides the pane.
        /// </summary>
        public void HidePane()
        {
            if (slideableView == null) return;

            slideableView.Visibility = ViewStates.Gone;
            RequestLayout();
        }
        
        /// <summary>
        /// Draw one child of this View Group.
        /// </summary>
        /// <param name="canvas">The canvas on which to draw the child</param>
        /// <param name="child">Who to draw</param>
        /// <param name="drawingTime">The time at which draw is occurring</param>
        /// <returns>
        /// To be added.
        /// </returns>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Draw one child of this View Group. This method is responsible for getting
        /// the canvas in the right state. This includes clipping, translating so
        /// that the child's scrolled origin is at 0, 0, and applying any animation
        /// transformations.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/ViewGroup.html#drawChild(android.graphics.Canvas, android.view.View, long)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        protected override bool DrawChild(Canvas canvas, View child, long drawingTime)
        {
            var lp = (LayoutParams)child.LayoutParameters;
            var save = canvas.Save(SaveFlags.Clip);

            var drawScrim = false;

            if (canSlide && !lp.Slideable && slideableView != null)
            {
                if (!OverlayContent)
                {
                    canvas.GetClipBounds(tmpRect);
                    if (isSlidingUp)
                        tmpRect.Bottom = Math.Min(tmpRect.Bottom, slideableView.Top);
                    else
                        tmpRect.Top = Math.Max(tmpRect.Top, slideableView.Bottom);

                    canvas.ClipRect(tmpRect);
                }

                if (slideOffset < 1)
                    drawScrim = true;
            }

            var result = base.DrawChild(canvas, child, drawingTime);
            canvas.RestoreToCount(save);

            if (drawScrim)
            {
                var baseAlpha = (coveredFadeColor.ToArgb() & 0xff000000) >> 24;
                var imag = (int)(baseAlpha * (1 - slideOffset));
                var color = imag << 24 | (coveredFadeColor.ToArgb() & 0xffffff);
                coveredFadePaint.Color = new Color(color);
                canvas.DrawRect(tmpRect, coveredFadePaint);
            }

            return result;
        }
        /// <summary>
        /// Produces a smooth slide animation.
        /// </summary>
        /// <param name="slideOffset">The slide offset.</param>
        /// <returns></returns>
        private bool SmoothSlideTo(float slideOffset)
        {
            if (!canSlide) return false;

            var y = isSlidingUp
                ? (int)(SlidingTop + slideOffset * slideRange)
                : (int)(SlidingTop - slideOffset * slideRange);

            if (!dragHelper.SmoothSlideViewTo(slideableView, slideableView.Left, y)) return false;

            SetAllChildrenVisible();
            ViewCompat.PostInvalidateOnAnimation(this);
            return true;
        }
        /// <summary>
        /// Called by a parent to request that a child update its values for mScrollX
        /// and mScrollY if necessary.
        /// </summary>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Called by a parent to request that a child update its values for mScrollX
        /// and mScrollY if necessary. This will typically be done if the child is
        /// animating a scroll using a <c><see cref="T:Android.Widget.Scroller" /></c>
        /// object.
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/View.html#computeScroll()" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        public override void ComputeScroll()
        {
            if (!dragHelper.ContinueSettling(true)) return;

            if (!canSlide)
            {
                dragHelper.Abort();
                return;
            }

            ViewCompat.PostInvalidateOnAnimation(this);
        }
        /// <summary>
        /// Manually render this view (and all of its children) to the given Canvas.
        /// </summary>
        /// <param name="canvas">The Canvas to which the View is rendered.</param>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Manually render this view (and all of its children) to the given Canvas.
        /// The view must have already done a full layout before this function is
        /// called.  When implementing a view, implement
        /// <c><see cref="M:Android.Views.View.OnDraw(Android.Graphics.Canvas)" /></c> instead of overriding this method.
        /// If you do need to override this method, call the superclass version.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/View.html#draw(android.graphics.Canvas)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);

            if (slideableView == null) return;
            if (ShadowDrawable == null) return;

            var right = slideableView.Right;
            var left = slideableView.Left;
            int top;
            int bottom;
            if (isSlidingUp)
            {
                top = slideableView.Top - shadowHeight;
                bottom = slideableView.Top;
            }
            else
            {
                top = slideableView.Bottom;
                bottom = slideableView.Bottom + shadowHeight;
            }

            ShadowDrawable.SetBounds(left, top, right, bottom);
            ShadowDrawable.Draw(canvas);
        }
        /// <summary>
        /// Determines whether this instance can scroll the specified view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="checkV">if set to <c>true</c> [check v].</param>
        /// <param name="dx">The dx.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        protected bool CanScroll(View view, bool checkV, int dx, int x, int y)
        {
            var viewGroup = view as ViewGroup;
            if (viewGroup == null) return checkV && ViewCompat.CanScrollHorizontally(view, -dx);

            var scrollX = viewGroup.ScrollX;
            var scrollY = viewGroup.ScrollY;
            var count = viewGroup.ChildCount;

            for (var i = count - 1; i >= 0; i--)
            {
                var child = viewGroup.GetChildAt(i);
                if (x + scrollX >= child.Left && x + scrollX < child.Right &&
                    y + scrollY >= child.Top && y + scrollY < child.Bottom &&
                    CanScroll(child, true, dx, x + scrollX - child.Left, y + scrollY - child.Top))
                    return true;
            }
            return checkV && ViewCompat.CanScrollHorizontally(view, -dx);
        }
        /// <summary>
        /// Returns a set of default layout parameters.
        /// </summary>
        /// <returns>
        /// To be added.
        /// </returns>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Returns a set of default layout parameters. These parameters are requested
        /// when the View passed to <c><see cref="M:Android.Views.ViewGroup.AddView(Android.Views.View)" /></c> has no layout parameters
        /// already set. If null is returned, an exception is thrown from addView.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/ViewGroup.html#generateDefaultLayoutParams()" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        protected override ViewGroup.LayoutParams GenerateDefaultLayoutParams()
        {
            return new LayoutParams();
        }
        /// <summary>
        /// Generates the layout parameters.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected override ViewGroup.LayoutParams GenerateLayoutParams(ViewGroup.LayoutParams p)
        {
            var param = p as MarginLayoutParams;
            return param != null ? new LayoutParams(param) : new LayoutParams(p);
        }
        /// <summary>
        /// Checks the layout parameters.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected override bool CheckLayoutParams(ViewGroup.LayoutParams p)
        {
            var param = p as LayoutParams;
            return param != null && base.CheckLayoutParams(p);
        }
        /// <summary>
        /// Returns a new set of layout parameters based on the supplied attributes set.
        /// </summary>
        /// <param name="attrs">the attributes to build the layout parameters from</param>
        /// <returns>
        /// To be added.
        /// </returns>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Returns a new set of layout parameters based on the supplied attributes set.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/ViewGroup.html#generateLayoutParams(android.util.AttributeSet)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        public override ViewGroup.LayoutParams GenerateLayoutParams(IAttributeSet attrs)
        {
            return new LayoutParams(Context, attrs);
        }
        /// <summary>
        /// Parameters for handling the margins of <see cref="SlidingUpPanelLayout"/>
        /// </summary>
        /// <seealso cref="Android.Views.ViewGroup.MarginLayoutParams" />
        public new class LayoutParams : MarginLayoutParams
        {
            /// <summary>
            /// The layout width attributes
            /// </summary>
            private static readonly int[] Attrs = {
                Android.Resource.Attribute.LayoutWidth
            };
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="LayoutParams"/> is slideable.
            /// </summary>
            /// <value>
            ///   <c>true</c> if slideable; otherwise, <c>false</c>.
            /// </value>
            public bool Slideable { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [dim when offset].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [dim when offset]; otherwise, <c>false</c>.
            /// </value>
            public bool DimWhenOffset { get; set; }
            /// <summary>
            /// Gets or sets the dim paint.
            /// </summary>
            /// <value>
            /// The dim paint.
            /// </value>
            public Paint DimPaint { get; set; }
            /// <summary>
            /// Initializes a new instance of the <see cref="LayoutParams"/> class.
            /// </summary>
            public LayoutParams()
                : base(MatchParent, MatchParent) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="LayoutParams"/> class.
            /// </summary>
            /// <param name="width">The width.</param>
            /// <param name="height">The height.</param>
            public LayoutParams(int width, int height)
                : base(width, height) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="LayoutParams"/> class.
            /// </summary>
            /// <param name="source">The source.</param>
            public LayoutParams(ViewGroup.LayoutParams source)
                : base(source) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="LayoutParams"/> class.
            /// </summary>
            /// <param name="source">The source.</param>
            public LayoutParams(MarginLayoutParams source)
                : base(source) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="LayoutParams"/> class.
            /// </summary>
            /// <param name="source">The source.</param>
            public LayoutParams(LayoutParams source)
                : base(source) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="LayoutParams"/> class.
            /// </summary>
            /// <param name="c">The c.</param>
            /// <param name="attrs">The attrs.</param>
            public LayoutParams(Context c, IAttributeSet attrs)
                : base(c, attrs)
            {
                var a = c.ObtainStyledAttributes(attrs, Attrs);
                a.Recycle();
            }
        }
        /// <summary>
        /// A callback object to help with Dragging views
        /// </summary>
        /// <seealso cref="Android.Support.V4.Widget.ViewDragHelper.Callback" />
        private class DragHelperCallback : ViewDragHelper.Callback
        {
            /// <summary>
            /// The panel layout
            /// </summary>
            private readonly SlidingUpPanelLayout panelLayout;

            public DragHelperCallback(SlidingUpPanelLayout layout)
            {
                panelLayout = layout;
            }
            /// <summary>
            /// Tries to capture the child view.
            /// </summary>
            /// <param name="child">The child.</param>
            /// <param name="pointerId">The pointer identifier.</param>
            /// <returns></returns>
            public override bool TryCaptureView(View child, int pointerId)
            {
                return !panelLayout.isUnableToDrag && ((LayoutParams)child.LayoutParameters).Slideable;
            }
            /// <summary>
            /// Called when [view drag state changed].
            /// </summary>
            /// <param name="state">The state.</param>
            public override void OnViewDragStateChanged(int state)
            {
                var anchoredTop = (int)(panelLayout.anchorPoint * panelLayout.slideRange);

                if (panelLayout.dragHelper.ViewDragState == ViewDragHelper.StateIdle)
                {
                    if (FloatNearlyEqual(panelLayout.slideOffset, 0))
                    {
                        if (panelLayout.slideState != SlideState.Expanded)
                        {
                            panelLayout.UpdateObscuredViewVisibility();
                            panelLayout.OnPanelExpanded(panelLayout.slideableView);
                            panelLayout.slideState = SlideState.Expanded;
                        }
                    }
                    else if (FloatNearlyEqual(panelLayout.slideOffset, (float)anchoredTop / panelLayout.slideRange))
                    {
                        if (panelLayout.slideState != SlideState.Anchored)
                        {
                            panelLayout.UpdateObscuredViewVisibility();
                            panelLayout.OnPanelAnchored(panelLayout.slideableView);
                            panelLayout.slideState = SlideState.Anchored;
                        }
                    }
                    else if (panelLayout.slideState != SlideState.Collapsed)
                    {
                        panelLayout.OnPanelCollapsed(panelLayout.slideableView);
                        panelLayout.slideState = SlideState.Collapsed;
                    }
                }
            }
            /// <summary>
            /// Called when [view captured].
            /// </summary>
            /// <param name="capturedChild">The captured child.</param>
            /// <param name="activePointerId">The active pointer identifier.</param>
            public override void OnViewCaptured(View capturedChild, int activePointerId)
            {
                panelLayout.SetAllChildrenVisible();
            }
            /// <summary>
            /// Called when [view position changed].
            /// </summary>
            /// <param name="changedView">The changed view.</param>
            /// <param name="left">The left.</param>
            /// <param name="top">The top.</param>
            /// <param name="dx">The dx.</param>
            /// <param name="dy">The dy.</param>
            public override void OnViewPositionChanged(View changedView, int left, int top, int dx, int dy)
            {
                panelLayout.OnPanelDragged(top);
                panelLayout.Invalidate();
            }
            /// <summary>
            /// Called when [view released].
            /// </summary>
            /// <param name="releasedChild">The released child.</param>
            /// <param name="xvel">The xvel.</param>
            /// <param name="yvel">The yvel.</param>
            public override void OnViewReleased(View releasedChild, float xvel, float yvel)
            {
                var top = panelLayout.isSlidingUp
                    ? panelLayout.SlidingTop
                    : panelLayout.SlidingTop - panelLayout.slideRange;

                if (!FloatNearlyEqual(panelLayout.anchorPoint, 0))
                {
                    int anchoredTop;
                    float anchorOffset;

                    if (panelLayout.isSlidingUp)
                    {
                        anchoredTop = (int)(panelLayout.anchorPoint * panelLayout.slideRange);
                        anchorOffset = (float)anchoredTop / panelLayout.slideRange;
                    }
                    else
                    {
                        anchoredTop = panelLayout.panelHeight -
                                      (int)(panelLayout.anchorPoint * panelLayout.slideRange);
                        anchorOffset = (float)(panelLayout.panelHeight - anchoredTop) / panelLayout.slideRange;
                    }

                    if (yvel > 0 || (FloatNearlyEqual(yvel, 0) && panelLayout.slideOffset >= (1f + anchorOffset) / 2))
                        top += panelLayout.slideRange;
                    else if (FloatNearlyEqual(yvel, 0) && panelLayout.slideOffset < (1f + anchorOffset) / 2 &&
                             panelLayout.slideOffset >= anchorOffset / 2)
                        top += (int)(panelLayout.slideRange * panelLayout.anchorPoint);
                }
                else if (yvel > 0 || (FloatNearlyEqual(yvel, 0) && panelLayout.slideOffset > 0.5f))
                    top += panelLayout.slideRange;

                panelLayout.dragHelper.SettleCapturedViewAt(releasedChild.Left, top);
                panelLayout.Invalidate();
            }
            /// <summary>
            /// Gets the view vertical drag range.
            /// </summary>
            /// <param name="child">The child.</param>
            /// <returns></returns>
            public override int GetViewVerticalDragRange(View child)
            {
                return panelLayout.slideRange;
            }
            /// <summary>
            /// Clamps the view position vertical.
            /// </summary>
            /// <param name="child">The child.</param>
            /// <param name="top">The top.</param>
            /// <param name="dy">The dy.</param>
            /// <returns></returns>
            public override int ClampViewPositionVertical(View child, int top, int dy)
            {
                int topBound;
                int bottomBound;
                if (panelLayout.isSlidingUp)
                {
                    topBound = panelLayout.SlidingTop;
                    bottomBound = topBound + panelLayout.slideRange;
                }
                else
                {
                    bottomBound = panelLayout.PaddingTop;
                    topBound = bottomBound - panelLayout.slideRange;
                }

                return Math.Min(Math.Max(top, topBound), bottomBound);
            }
        }
        /// <summary>
        /// Hook allowing a view to generate a representation of its internal state
        /// that can later be used to create a new instance with that same state.
        /// </summary>
        /// <returns>
        /// To be added.
        /// </returns>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Hook allowing a view to generate a representation of its internal state
        /// that can later be used to create a new instance with that same state.
        /// This state should only contain information that is not persistent or can
        /// not be reconstructed later. For example, you will never store your
        /// current position on screen because that will be computed again when a
        /// new instance of the view is placed in its view hierarchy.
        /// </para>
        /// <para tool="javadoc-to-mdoc">
        /// Some examples of things you may store here: the current cursor position
        /// in a text view (but usually not the text itself since that is stored in a
        /// content provider or other persistent storage), the currently selected
        /// item in a list view.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/View.html#onSaveInstanceState()" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        /// <altmember cref="M:Android.Views.View.OnRestoreInstanceState(Android.OS.IParcelable)" />
        /// <altmember cref="P:Android.Views.View.SaveEnabled" />
        protected override IParcelable OnSaveInstanceState()
        {
            var superState = base.OnSaveInstanceState();

            var savedState = new SavedState(superState, slideState);
            return savedState;
        }
        /// <summary>
        /// Hook allowing a view to re-apply a representation of its internal state that had previously
        /// been generated by <c><see cref="M:Android.Views.View.OnSaveInstanceState" /></c>.
        /// </summary>
        /// <param name="state">The frozen state that had previously been returned by
        /// <c><see cref="M:Android.Views.View.OnSaveInstanceState" /></c>.</param>
        /// <remarks>
        /// <para tool="javadoc-to-mdoc">Hook allowing a view to re-apply a representation of its internal state that had previously
        /// been generated by <c><see cref="M:Android.Views.View.OnSaveInstanceState" /></c>. This function will never be called with a
        /// null state.</para>
        /// <para tool="javadoc-to-mdoc">
        ///   <format type="text/html">
        ///     <a href="http://developer.android.com/reference/android/view/View.html#onRestoreInstanceState(android.os.Parcelable)" target="_blank">[Android Documentation]</a>
        ///   </format>
        /// </para>
        /// </remarks>
        /// <since version="Added in API level 1" />
        /// <altmember cref="M:Android.Views.View.OnSaveInstanceState" />
        protected override void OnRestoreInstanceState(IParcelable state)
        {
            try
            {
                var savedState = (SavedState)state;
                base.OnRestoreInstanceState(savedState.SuperState);
                slideState = savedState.State;
            }
            catch
            {
                base.OnRestoreInstanceState(state);
            }
        }

        public class SavedState : BaseSavedState
        {
            /// <summary>
            /// Gets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public SlideState State { get; private set; }
            /// <summary>
            /// Initializes a new instance of the <see cref="SavedState"/> class.
            /// </summary>
            /// <param name="superState">State of the super.</param>
            /// <param name="item">The item.</param>
            public SavedState(IParcelable superState, SlideState item)
                : base(superState)
            {
                State = item;
            }
            /// <summary>
            /// Initializes a new instance of the <see cref="SavedState"/> class.
            /// </summary>
            /// <param name="parcel">The parcel.</param>
            public SavedState(Parcel parcel)
                : base(parcel)
            {
                try
                {
                    State = (SlideState)parcel.ReadInt();
                }
                catch
                {
                    State = SlideState.Collapsed;
                }
            }
            /// <summary>
            /// Flatten this object in to a Parcel.
            /// </summary>
            /// <param name="dest">The Parcel in which the object should be written.</param>
            /// <param name="flags">Additional flags about how the object should be written.
            /// May be 0 or <c><see cref="F:Android.OS.Parcelable.ParcelableWriteReturnValue" /></c>.</param>
            /// <remarks>
            /// <para tool="javadoc-to-mdoc">Flatten this object in to a Parcel.</para>
            /// <para tool="javadoc-to-mdoc">
            ///   <format type="text/html">
            ///     <a href="http://developer.android.com/reference/android/view/AbsSavedState.html#writeToParcel(android.os.Parcel, int)" target="_blank">[Android Documentation]</a>
            ///   </format>
            /// </para>
            /// </remarks>
            /// <since version="Added in API level 1" />
            public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
            {
                base.WriteToParcel(dest, flags);
                dest.WriteInt((int)State);
            }
            /// <summary>
            /// Initializes the creator.
            /// </summary>
            /// <returns></returns>
            [ExportField("CREATOR")]
            static SavedStateCreator InitializeCreator()
            {
                return new SavedStateCreator();
            }
            /// <summary>
            /// 
            /// </summary>
            /// <seealso cref="Java.Lang.Object" />
            /// <seealso cref="Android.OS.IParcelableCreator" />
            class SavedStateCreator : Java.Lang.Object, IParcelableCreator
            {
                /// <summary>
                /// Create a new instance of the Parcelable class, instantiating it
                /// from the given Parcel whose data had previously been written by
                /// <c><see cref="!:Android.OS.Parcelable.writeToParcel(android.os.Parcel, int)" /></c>.
                /// </summary>
                /// <param name="source">The Parcel to read the object's data from.</param>
                /// <returns>
                /// To be added.
                /// </returns>
                /// <remarks>
                /// <para tool="javadoc-to-mdoc">Create a new instance of the Parcelable class, instantiating it
                /// from the given Parcel whose data had previously been written by
                /// <c><see cref="!:Android.OS.Parcelable.writeToParcel(android.os.Parcel, int)" /></c>.</para>
                /// <para tool="javadoc-to-mdoc">
                ///   <format type="text/html">
                ///     <a href="http://developer.android.com/reference/android/os/Parcelable.Creator.html#createFromParcel(android.os.Parcel)" target="_blank">[Android Documentation]</a>
                ///   </format>
                /// </para>
                /// </remarks>
                /// <since version="Added in API level 1" />
                public Java.Lang.Object CreateFromParcel(Parcel source)
                {
                    return new SavedState(source);
                }
                /// <summary>
                /// Create a new array of the Parcelable class.
                /// </summary>
                /// <param name="size">Size of the array.</param>
                /// <returns>
                /// To be added.
                /// </returns>
                /// <remarks>
                /// <para tool="javadoc-to-mdoc">Create a new array of the Parcelable class.</para>
                /// <para tool="javadoc-to-mdoc">
                ///   <format type="text/html">
                ///     <a href="http://developer.android.com/reference/android/os/Parcelable.Creator.html#newArray(int)" target="_blank">[Android Documentation]</a>
                ///   </format>
                /// </para>
                /// </remarks>
                /// <since version="Added in API level 1" />
                public Java.Lang.Object[] NewArray(int size)
                {
                    return new SavedState[size];
                }
            }
        }
        /// <summary>
        /// Comarator to determine if <paramref="a"/> and <param name="b"/> differ by a maximum of <paramref name="epsilon"/>
        /// or not.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        public static bool FloatNearlyEqual(float a, float b, float epsilon)
        {
            var absA = Math.Abs(a);
            var absB = Math.Abs(b);
            var diff = Math.Abs(a - b);

            if (a == b) // shortcut, handles infinities
                return true;
            if (a == 0 || b == 0 || diff < float.MinValue)
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * float.MinValue);

            // use relative error
            return diff / (absA + absB) < epsilon;
        }
        /// <summary>
        /// Comarator to determine if <paramref="a"/> and <param name="b"/> differ by a maximum of <paramref name="epsilon"/>
        /// or not.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        public static bool FloatNearlyEqual(float a, float b)
        {
            return FloatNearlyEqual(a, b, 0.00001f);
        }
    }
}