// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.ObjectModel;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using NorthwindPhoto.Model;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace NorthwindPhoto
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Gallery : Page
    {
        private int _animationDuration = 400;
        private Compositor _compositor;

        public Gallery()
        {
            InitializeComponent();
        }

        /// <summary>
        /// TODO: 1.Show/Hide
        /// When the image is being loaded into the container a show/hide animation is being applied
        /// </summary>
        private void PhotoCollectionViewer_ChoosingItemContainer(ListViewBase sender,
            ChoosingItemContainerEventArgs args)
        {
            args.ItemContainer = args.ItemContainer ?? new GridViewItem();

            var fadeIn = _compositor.CreateScalarKeyFrameAnimation();
            fadeIn.Target = "Opacity";
            fadeIn.Duration = TimeSpan.FromMilliseconds(_animationDuration);
            fadeIn.InsertKeyFrame(0, 0);
            fadeIn.InsertKeyFrame(1, 1);
            fadeIn.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
            fadeIn.DelayTime = TimeSpan.FromMilliseconds(_animationDuration * 0.125 * args.ItemIndex);

            var fadeOut = _compositor.CreateScalarKeyFrameAnimation();
            fadeOut.Target = "Opacity";
            fadeOut.Duration = TimeSpan.FromMilliseconds(_animationDuration);
            fadeOut.InsertKeyFrame(1, 0);

            var scaleIn = _compositor.CreateVector3KeyFrameAnimation();
            scaleIn.Target = "Scale";
            scaleIn.Duration = TimeSpan.FromMilliseconds(_animationDuration);
            scaleIn.InsertKeyFrame(0f, new Vector3(1.2f, 1.2f, 1.2f));
            scaleIn.InsertKeyFrame(1f, new Vector3(1f, 1f, 1f));
            scaleIn.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
            scaleIn.DelayTime = TimeSpan.FromMilliseconds(_animationDuration * 0.125 * args.ItemIndex);

            // animations set to run at the same time are grouped
            var animationFadeInGroup = _compositor.CreateAnimationGroup();
            animationFadeInGroup.Add(fadeIn);
            animationFadeInGroup.Add(scaleIn);

            // Set up show and hide animations for this item container before the element is added to the tree.
            // These fire when items are added/removed from the visual tree, including when you set Visibilty
            ElementCompositionPreview.SetImplicitShowAnimation(args.ItemContainer, animationFadeInGroup);
            ElementCompositionPreview.SetImplicitHideAnimation(args.ItemContainer, fadeOut);
        }

        #region Pointer Entered / Exit

        public ObservableCollection<Photo> Photos { get; set; } = App.PhotoCollection;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;

            if (e.Parameter != null)
                int.TryParse(e.Parameter.ToString(), out _animationDuration);
        }

        private void PhotoCollectionViewer_ItemClick(object sender, ItemClickEventArgs e)
        {
            App.MainFrame.Navigate(typeof(ImageEditingPage), e.ClickedItem as Photo);
        }

        private void GalleryItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            ScaleImage(frameworkElement, 1f);
        }

        private void GalleryItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            ScaleImage(frameworkElement, 1.1f);
        }

        private void ScaleImage(FrameworkElement frameworkElement, float scaleAmout)
        {
            var element = ElementCompositionPreview.GetElementVisual(frameworkElement);

            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);
            scaleAnimation.InsertKeyFrame(1f, new Vector3(scaleAmout, scaleAmout, scaleAmout));

            element.CenterPoint = new Vector3((float) frameworkElement.ActualHeight / 2,
                (float) frameworkElement.ActualWidth / 2, 275f / 2);
            element.StartAnimation("Scale", scaleAnimation);

            if (scaleAmout > 1)
            {
                var shadow = _compositor.CreateDropShadow();
                shadow.Offset = new Vector3(15, 15, -10);
                shadow.BlurRadius = 5;
                shadow.Color = Colors.DarkGray;

                var sprite = _compositor.CreateSpriteVisual();
                sprite.Size = new Vector2((float) frameworkElement.ActualWidth - 20,
                    (float) frameworkElement.ActualHeight - 20);
                sprite.Shadow = shadow;

                ElementCompositionPreview.SetElementChildVisual((UIElement) frameworkElement.FindName("Shadow"), sprite);
            }
            else
            {
                ElementCompositionPreview.SetElementChildVisual((UIElement) frameworkElement.FindName("Shadow"), null);
            }
        }

        #endregion
    }
}