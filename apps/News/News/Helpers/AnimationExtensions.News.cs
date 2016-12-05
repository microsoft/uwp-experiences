using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace News.Helpers
{
    public static class AnimationExtensions
    {
        public static AnimationSet Reveal(
            this UIElement associatedObject,
            double duration = 500d,
            double delay = 0d)
        {
            if (associatedObject == null)
            {
                return null;
            }

            var animationSet = new AnimationSet(associatedObject);
            return animationSet.Reveal(duration, delay);
        }

        public static AnimationSet Reveal(
            this AnimationSet animationSet,
            double duration = 500d,
            double delay = 0d)
        {
            if (animationSet == null)
            {
                return null;
            }

            var element = animationSet.Element;
            var rectangleTransform = element.Clip.Transform as CompositeTransform;

            if (rectangleTransform == null)
            {
                return animationSet;
            }

            var animationX = new DoubleAnimation();
            var animationY = new DoubleAnimation();

            animationX.To = 1;
            animationY.To = 1;

            animationX.Duration = animationY.Duration = TimeSpan.FromMilliseconds(duration);
            animationX.BeginTime = animationY.BeginTime = TimeSpan.FromMilliseconds(delay);
            animationX.EasingFunction = animationY.EasingFunction = new CubicEase();

            animationSet.AddStoryboardAnimation("(UIElement.Clip).(RectangleGeometry.Transform).(CompositeTransform.ScaleX)", animationX);
            animationSet.AddStoryboardAnimation("(UIElement.Clip).(RectangleGeometry.Transform).(CompositeTransform.ScaleY)", animationY);

            return animationSet;
        }

    }
}
