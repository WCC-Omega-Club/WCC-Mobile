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
        private new readonly string Tag = "SlidingUpPanelLayout";
        private const int DefaultPanelHeight = 68;
        private const int DefaultShadowHeight = 4;
        private const int DefaultMinFlingVelocity = 400;
        private const bool DefaultOverlayFlag = false;
        private static readonly Color DefaultFadeColor = new Color(0, 0, 0, 99);
        private static readonly int[] DefaultAttrs = { Android.Resource.Attribute.Gravity };

        private readonly int minFlingVelocity = DefaultMinFlingVelocity;
        private Color coveredFadeColor = DefaultFadeColor;
        private readonly Paint coveredFadePaint = new Paint();
        private int panelHeight = -1;
        private readonly int shadowHeight = -1;
        private readonly bool isSlidingUp;
        private bool canSlide;
        private View dragView;
        private readonly int dragViewResId = -1;
        private View slideableView;
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

        public event SlidingUpPanelSlideEventHandler PanelSlide;
        public event SlidingUpPanelEventHandler PanelCollapsed;
        public event SlidingUpPanelEventHandler PanelExpanded;
        public event SlidingUpPanelEventHandler PanelAnchored;
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
        //
        public float AnchorPoint
        {
            get { return anchorPoint; }
            set
            {
                if (value > 0 && value < 1)
                    anchorPoint = value;
            }
        }

        public Drawable ShadowDrawable { get; set; }

        public bool SlidingEnabled { get; set; }

        public bool OverlayContent { get; set; }

        public bool IsUsingDragViewTouchEvents { get; set; }

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

        public SlidingUpPanelLayout(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public SlidingUpPanelLayout(Context context)
            : this(context, null) { }

        public SlidingUpPanelLayout(Context context, IAttributeSet attrs)
            : this(context, attrs, 0) { }

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

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            if (dragViewResId != -1)
                dragView = FindViewById(dragViewResId);
        }

        private void OnPanelSlide(View panel)
        {
            if (PanelSlide != null)
                PanelSlide(this, new SlidingUpPanelSlideEventArgs { Panel = panel, SlideOffset = slideOffset });
        }

        private void OnPanelCollapsed(View panel)
        {
            if (PanelCollapsed != null)
                PanelCollapsed(this, new SlidingUpPanelEventArgs { Panel = panel });
            SendAccessibilityEvent(EventTypes.WindowStateChanged);
        }

        private void OnPanelAnchored(View panel)
        {
            if (PanelAnchored != null)
                PanelAnchored(this, new SlidingUpPanelEventArgs { Panel = panel });
            SendAccessibilityEvent(EventTypes.WindowStateChanged);
        }

        private void OnPanelExpanded(View panel)
        {
            if (PanelExpanded != null)
                PanelExpanded(this, new SlidingUpPanelEventArgs { Panel = panel });
            SendAccessibilityEvent(EventTypes.WindowStateChanged);
        }

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

        private void SetAllChildrenVisible()
        {
            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (child.Visibility == ViewStates.Invisible)
                    child.Visibility = ViewStates.Visible;
            }
        }

        private static bool HasOpaqueBackground(View view)
        {
            var bg = view.Background;
            if (bg != null)
                return bg.Opacity == (int)Format.Opaque;
            return false;
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            firstLayout = true;
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            firstLayout = true;
        }

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

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            if (h != oldh)
                firstLayout = true;
        }

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

        public bool CollapsePane()
        {
            if (firstLayout || SmoothSlideTo(1.0f))
                return true;
            return false;
        }

        public bool ExpandPane()
        {
            return ExpandPane(0);
        }

        public bool ExpandPane(float slideOffset)
        {
            if (!PaneVisible)
                ShowPane();
            return firstLayout || SmoothSlideTo(slideOffset);
        }

        public void ShowPane()
        {
            if (ChildCount < 2) return;

            var slidingPane = GetChildAt(1);
            slidingPane.Visibility = ViewStates.Visible;
            RequestLayout();
        }

        public void HidePane()
        {
            if (slideableView == null) return;

            slideableView.Visibility = ViewStates.Gone;
            RequestLayout();
        }

        private void OnPanelDragged(int newTop)
        {
            slideOffset = isSlidingUp
                ? (float)(newTop - SlidingTop) / slideRange
                : (float)(SlidingTop - newTop) / slideRange;
            OnPanelSlide(slideableView);
        }

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

        protected override ViewGroup.LayoutParams GenerateDefaultLayoutParams()
        {
            return new LayoutParams();
        }

        protected override ViewGroup.LayoutParams GenerateLayoutParams(ViewGroup.LayoutParams p)
        {
            var param = p as MarginLayoutParams;
            return param != null ? new LayoutParams(param) : new LayoutParams(p);
        }

        protected override bool CheckLayoutParams(ViewGroup.LayoutParams p)
        {
            var param = p as LayoutParams;
            return param != null && base.CheckLayoutParams(p);
        }

        public override ViewGroup.LayoutParams GenerateLayoutParams(IAttributeSet attrs)
        {
            return new LayoutParams(Context, attrs);
        }

        public new class LayoutParams : MarginLayoutParams
        {
            private static readonly int[] Attrs = {
                Android.Resource.Attribute.LayoutWidth
            };

            public bool Slideable { get; set; }

            public bool DimWhenOffset { get; set; }

            public Paint DimPaint { get; set; }

            public LayoutParams()
                : base(MatchParent, MatchParent) { }

            public LayoutParams(int width, int height)
                : base(width, height) { }

            public LayoutParams(ViewGroup.LayoutParams source)
                : base(source) { }

            public LayoutParams(MarginLayoutParams source)
                : base(source) { }

            public LayoutParams(LayoutParams source)
                : base(source) { }

            public LayoutParams(Context c, IAttributeSet attrs)
                : base(c, attrs)
            {
                var a = c.ObtainStyledAttributes(attrs, Attrs);
                a.Recycle();
            }
        }

        private class DragHelperCallback : ViewDragHelper.Callback
        {
            //This class is a bit nasty, as C# does not allow calling variables directly
            //like stupid Java does.
            private readonly SlidingUpPanelLayout _panelLayout;

            public DragHelperCallback(SlidingUpPanelLayout layout)
            {
                _panelLayout = layout;
            }

            public override bool TryCaptureView(View child, int pointerId)
            {
                return !_panelLayout.isUnableToDrag && ((LayoutParams)child.LayoutParameters).Slideable;
            }

            public override void OnViewDragStateChanged(int state)
            {
                var anchoredTop = (int)(_panelLayout.anchorPoint * _panelLayout.slideRange);

                if (_panelLayout.dragHelper.ViewDragState == ViewDragHelper.StateIdle)
                {
                    if (FloatNearlyEqual(_panelLayout.slideOffset, 0))
                    {
                        if (_panelLayout.slideState != SlideState.Expanded)
                        {
                            _panelLayout.UpdateObscuredViewVisibility();
                            _panelLayout.OnPanelExpanded(_panelLayout.slideableView);
                            _panelLayout.slideState = SlideState.Expanded;
                        }
                    }
                    else if (FloatNearlyEqual(_panelLayout.slideOffset, (float)anchoredTop / _panelLayout.slideRange))
                    {
                        if (_panelLayout.slideState != SlideState.Anchored)
                        {
                            _panelLayout.UpdateObscuredViewVisibility();
                            _panelLayout.OnPanelAnchored(_panelLayout.slideableView);
                            _panelLayout.slideState = SlideState.Anchored;
                        }
                    }
                    else if (_panelLayout.slideState != SlideState.Collapsed)
                    {
                        _panelLayout.OnPanelCollapsed(_panelLayout.slideableView);
                        _panelLayout.slideState = SlideState.Collapsed;
                    }
                }
            }

            public override void OnViewCaptured(View capturedChild, int activePointerId)
            {
                _panelLayout.SetAllChildrenVisible();
            }

            public override void OnViewPositionChanged(View changedView, int left, int top, int dx, int dy)
            {
                _panelLayout.OnPanelDragged(top);
                _panelLayout.Invalidate();
            }

            public override void OnViewReleased(View releasedChild, float xvel, float yvel)
            {
                var top = _panelLayout.isSlidingUp
                    ? _panelLayout.SlidingTop
                    : _panelLayout.SlidingTop - _panelLayout.slideRange;

                if (!FloatNearlyEqual(_panelLayout.anchorPoint, 0))
                {
                    int anchoredTop;
                    float anchorOffset;

                    if (_panelLayout.isSlidingUp)
                    {
                        anchoredTop = (int)(_panelLayout.anchorPoint * _panelLayout.slideRange);
                        anchorOffset = (float)anchoredTop / _panelLayout.slideRange;
                    }
                    else
                    {
                        anchoredTop = _panelLayout.panelHeight -
                                      (int)(_panelLayout.anchorPoint * _panelLayout.slideRange);
                        anchorOffset = (float)(_panelLayout.panelHeight - anchoredTop) / _panelLayout.slideRange;
                    }

                    if (yvel > 0 || (FloatNearlyEqual(yvel, 0) && _panelLayout.slideOffset >= (1f + anchorOffset) / 2))
                        top += _panelLayout.slideRange;
                    else if (FloatNearlyEqual(yvel, 0) && _panelLayout.slideOffset < (1f + anchorOffset) / 2 &&
                             _panelLayout.slideOffset >= anchorOffset / 2)
                        top += (int)(_panelLayout.slideRange * _panelLayout.anchorPoint);
                }
                else if (yvel > 0 || (FloatNearlyEqual(yvel, 0) && _panelLayout.slideOffset > 0.5f))
                    top += _panelLayout.slideRange;

                _panelLayout.dragHelper.SettleCapturedViewAt(releasedChild.Left, top);
                _panelLayout.Invalidate();
            }

            public override int GetViewVerticalDragRange(View child)
            {
                return _panelLayout.slideRange;
            }

            public override int ClampViewPositionVertical(View child, int top, int dy)
            {
                int topBound;
                int bottomBound;
                if (_panelLayout.isSlidingUp)
                {
                    topBound = _panelLayout.SlidingTop;
                    bottomBound = topBound + _panelLayout.slideRange;
                }
                else
                {
                    bottomBound = _panelLayout.PaddingTop;
                    topBound = bottomBound - _panelLayout.slideRange;
                }

                return Math.Min(Math.Max(top, topBound), bottomBound);
            }
        }

        protected override IParcelable OnSaveInstanceState()
        {
            var superState = base.OnSaveInstanceState();

            var savedState = new SavedState(superState, slideState);
            return savedState;
        }

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
            public SlideState State { get; private set; }

            public SavedState(IParcelable superState, SlideState item)
                : base(superState)
            {
                State = item;
            }

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

            public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
            {
                base.WriteToParcel(dest, flags);
                dest.WriteInt((int)State);
            }

            [ExportField("CREATOR")]
            static SavedStateCreator InitializeCreator()
            {
                return new SavedStateCreator();
            }

            class SavedStateCreator : Java.Lang.Object, IParcelableCreator
            {
                public Java.Lang.Object CreateFromParcel(Parcel source)
                {
                    return new SavedState(source);
                }

                public Java.Lang.Object[] NewArray(int size)
                {
                    return new SavedState[size];
                }
            }
        }

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

        public static bool FloatNearlyEqual(float a, float b)
        {
            return FloatNearlyEqual(a, b, 0.00001f);
        }
    }
}