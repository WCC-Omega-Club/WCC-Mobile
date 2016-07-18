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
    public static class AnimationExtensions
    {
        /// <summary>
        /// The current animations
        /// </summary>
        static Dictionary<View, Animator> currentAnimations = new Dictionary<View, Animator>();
        /// <summary>
        /// Clears the old animation.
        /// </summary>
        /// <param name="view">The view.</param>
        static void ClearOldAnimation(View view)
        {
            Animator oldAnimator;
            if (currentAnimations.TryGetValue(view, out oldAnimator))
            {
                oldAnimator.Cancel();
                currentAnimations.Remove(view);
            }
        }
        /// <summary>
        /// Create a new animation using current animations.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="endAction">The end action.</param>
        /// <param name="startDelay">The start delay.</param>
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
        /// <summary>
        /// Translates the <paramref name="view"/> in the y-axis.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="translation">The translation.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="interpolator">The interpolator.</param>
        /// <param name="endAction">The end action.</param>
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