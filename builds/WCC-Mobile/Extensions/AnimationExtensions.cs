using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Runnable = Java.Lang.Runnable;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.Animations;
using Android.Transitions;
using System.IO;
using Android.Support.V4.View;
using Android.Animation;

namespace WCCMobile
{
    /*public static class AnimationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="endAction">The end action.</param>
        /// <param name="startDelay">The start delay.</param>
        public static void AlphaAnimate(this View view, float alpha, int duration = 300, Action endAction = null, int startDelay = 0)
        {
            var animator = ViewCompat.Animate(view);
            animator
                .SetDuration(duration)
                .SetStartDelay(startDelay)
                .Alpha(alpha);
            if (endAction != null)
                animator.WithEndAction(new Runnable(endAction));
            animator.Start();
        }
        /// <summary>
        /// Translations the y animate.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="translation">The translation.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="interpolator">The interpolator.</param>
        /// <param name="endAction">The end action.</param>
        public static void TranslationYAnimate(this View view, int translation, int duration = 300,
                                                IInterpolator interpolator = null, Action endAction = null)
        {
            var animator = ViewCompat.Animate(view);
            animator
                .SetDuration(duration)
                .TranslationY(translation);
            if (endAction != null)
                animator.WithEndAction(new Runnable(endAction));
            if (interpolator != null)
                animator.SetInterpolator(interpolator);
            animator.Start();
        }
        /// <summary>
        /// Setups the fragment transitions.
        /// </summary>
        /// <param name="frag">The frag.</param>
        public static void SetupFragmentTransitions(Android.Support.V4.App.Fragment frag)
        {
            if (!AndroidExtensions.IsMaterial)
                return;
            frag.EnterTransition = new Slide(GravityFlags.Left);
            frag.ExitTransition = new Fade(FadingMode.Out);
        }
        /// <summary>
        /// Setups the automatic scene transition.
        /// </summary>
        /// <param name="root">The root.</param>
        public static void SetupAutoSceneTransition(ViewGroup root)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
                TransitionManager.BeginDelayedTransition(root);
        }
    }*/

    public static class AnimationExtensions
    {
        static Dictionary<View, Animator> currentAnimations = new Dictionary<View, Animator>();

        static void ClearOldAnimation(View view)
        {
            Animator oldAnimator;
            if (currentAnimations.TryGetValue(view, out oldAnimator))
            {
                oldAnimator.Cancel();
                currentAnimations.Remove(view);
            }
        }

        public static void AlphaAnimate(this View view, float alpha, int duration = 300, Action endAction = null, int startDelay = 0)
        {
            ClearOldAnimation(view);
            var animator = ObjectAnimator.OfFloat(view, "alpha", view.Alpha, alpha);
            currentAnimations[view] = animator;
            animator.SetDuration(duration);
            animator.StartDelay = startDelay;
            animator.AnimationEnd += (sender, e) => {
                currentAnimations.Remove(view);
                if (endAction != null)
                    endAction();
                ((Animator)sender).RemoveAllListeners();
            };
            animator.Start();
        }

        public static void TranslationYAnimate(this View view, int translation, int duration = 300,
                                                ITimeInterpolator interpolator = null, Action endAction = null)
        {
            ClearOldAnimation(view);
            var animator = ObjectAnimator.OfFloat(view, "translationY", view.TranslationY, translation);
            animator.SetDuration(duration);
            if (interpolator != null)
                animator.SetInterpolator(interpolator);
            animator.AnimationEnd += (sender, e) => {
                currentAnimations.Remove(view);
                if (endAction != null)
                    endAction();
                ((Animator)sender).RemoveAllListeners();
            };
            animator.Start();
        }

    }

}