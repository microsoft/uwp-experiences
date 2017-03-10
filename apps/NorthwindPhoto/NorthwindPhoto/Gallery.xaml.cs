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
using Windows.Foundation;
using Windows.UI.Xaml.Media.Animation;
using System.Diagnostics;

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
        private Visual _gridview;

        private SpotLight spotLight;
        private float spotlight_z = 300;
        private AmbientLight ambientLight;
        private Color ambientLightColorOn = Color.FromArgb(255, 220, 220, 220);
        private Color ambientLightColorOff = Color.FromArgb(255, 255, 255, 255);

        private float durationHover = 500;
        private float shadowDistance = 5;
        private static int persistedItemIndex = -1;

        public Gallery()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Hook up lights for hover
        /// </summary>
        private void PhotoCollectionViewer_ChoosingItemContainer(ListViewBase sender,
            ChoosingItemContainerEventArgs args)
        {
            bool isInCollection = false;
            args.ItemContainer = args.ItemContainer ?? new GridViewItem();

            foreach (Visual v in spotLight.Targets)
            {
                if (v == ElementCompositionPreview.GetElementVisual(args.ItemContainer))
                {
                    isInCollection = true;
                    break;
                }
            }

            if (!isInCollection)
            {
                spotLight.Targets.Add(ElementCompositionPreview.GetElementVisual(args.ItemContainer));
                ambientLight.Targets.Add(ElementCompositionPreview.GetElementVisual(args.ItemContainer));
            }

        }

        #region Staggering animation
        private void PhotoCollectionViewer_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Loaded += ItemContainer_Loaded;
        }

        private void ItemContainer_Loaded(object sender, RoutedEventArgs e)
        {
            var itemsPanel = (ItemsWrapGrid)this.PhotoCollectionViewer.ItemsPanelRoot;
            var itemContainer = (GridViewItem)sender;

            var itemIndex = this.PhotoCollectionViewer.IndexFromContainer(itemContainer);

            var relativeIndex = itemIndex - itemsPanel.FirstVisibleIndex;

            var uc = itemContainer.ContentTemplateRoot as Grid;

            if (itemIndex != persistedItemIndex && itemIndex >= 0 && itemIndex >= itemsPanel.FirstVisibleIndex && itemIndex <= itemsPanel.LastVisibleIndex)
            {
                var itemVisual = ElementCompositionPreview.GetElementVisual(uc);
                ElementCompositionPreview.SetIsTranslationEnabled(uc, true);

                // Create KeyFrameAnimations
                KeyFrameAnimation offsetAnimation = _compositor.CreateScalarKeyFrameAnimation();
                offsetAnimation.InsertExpressionKeyFrame(0f, "100");
                offsetAnimation.InsertExpressionKeyFrame(1f, "0");
                offsetAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
                offsetAnimation.Duration = TimeSpan.FromMilliseconds(1800);
                offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(relativeIndex * 50);

                KeyFrameAnimation fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
                fadeAnimation.InsertExpressionKeyFrame(0f, "0");
                fadeAnimation.InsertExpressionKeyFrame(1f, "1");
                fadeAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
                fadeAnimation.Duration = TimeSpan.FromMilliseconds(1800);
                fadeAnimation.DelayTime = TimeSpan.FromMilliseconds(relativeIndex * 50);

                // Start animations
                itemVisual.StartAnimation("Offset.Y", offsetAnimation);
                //itemVisual.StartAnimation("Scale", scaleAnimation);
                itemVisual.StartAnimation("Opacity", fadeAnimation);
            }
            else
            {
                Debug.WriteLine("sss");
            }

            itemContainer.Loaded -= this.ItemContainer_Loaded;
        } 
        #endregion

        #region Pointer Entered / Exit

        public ObservableCollection<Photo> Photos { get; set; } = App.PhotoCollection;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;

            if (e.Parameter != null)
                int.TryParse(e.Parameter.ToString(), out _animationDuration);

            _gridview = ElementCompositionPreview.GetElementVisual(this);
            ApplyLighting();

            PhotoCollectionViewer.Loaded += async (o_, e_) =>
            {
                var connectedAnimation = ConnectedAnimationService
                    .GetForCurrentView()
                    .GetAnimation("Image");
                if (connectedAnimation != null)
                {
                    var item = PhotoCollectionViewer.Items[persistedItemIndex];
                    PhotoCollectionViewer.ScrollIntoView(item);
                    await PhotoCollectionViewer.TryStartConnectedAnimationAsync(
                        connectedAnimation,
                        item,
                        "Image"
                    );
                }
            };
        }

        private void PhotoCollectionViewer_ItemClick(object sender, ItemClickEventArgs e)
        {
            ConnectedAnimationService.GetForCurrentView().DefaultDuration = TimeSpan.FromSeconds(0.5);

            persistedItemIndex = PhotoCollectionViewer.Items.IndexOf(e.ClickedItem);
            PhotoCollectionViewer.PrepareConnectedAnimation("Image", e.ClickedItem, "Image");


            Canvas.SetZIndex(this, 1);
            var animation = _compositor.CreateScalarKeyFrameAnimation();
            animation.Target = "Opacity";
            animation.Duration = TimeSpan.FromSeconds(0.6);
            animation.InsertKeyFrame(1, 0);
            ElementCompositionPreview.SetImplicitHideAnimation(this, animation);

            App.MainFrame.Navigate(typeof(ImageEditingPage), e.ClickedItem as Photo);
        }

        private void GalleryItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var element = ElementCompositionPreview.GetElementVisual((UIElement)sender);

            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(durationHover);
            scaleAnimation.InsertKeyFrame(1f, new Vector3(1f, 1f, 1f));

            element.CenterPoint = new Vector3(275f / 2, 275f / 2, 275f / 2);
            element.StartAnimation("Scale", scaleAnimation);

            var shadowBorder = (Grid)sender;
            ElementCompositionPreview.SetElementChildVisual((UIElement)shadowBorder.FindName("Shadow"), null);

            // Update AmbientLight when the item is no longer hovered on

            var ambientLightColorAnimation = _compositor.CreateColorKeyFrameAnimation();
            ambientLightColorAnimation.Duration = TimeSpan.FromMilliseconds(durationHover);
            ambientLightColorAnimation.InsertKeyFrame(1f, ambientLightColorOff);
            ambientLight.StartAnimation(nameof(AmbientLight.Color), ambientLightColorAnimation);
        }

        private void GalleryItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;

            //Setup Animations for the current Image Visual

            var element = ElementCompositionPreview.GetElementVisual(frameworkElement);
            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(durationHover);
            scaleAnimation.InsertKeyFrame(1f, new Vector3(1.1f, 1.1f, 1.1f));

            element.CenterPoint = new Vector3(275f / 2, 275f / 2, 275f / 2);
            element.StartAnimation("Scale", scaleAnimation);

            var shadowBorder = (Grid)sender;
            var shadow = _compositor.CreateDropShadow();
            shadow.Color = Colors.DimGray;

            var shadowBlurAnimation = _compositor.CreateScalarKeyFrameAnimation();
            shadowBlurAnimation.Duration = TimeSpan.FromMilliseconds(durationHover);
            shadowBlurAnimation.InsertKeyFrame(0f, 1f);
            shadowBlurAnimation.InsertKeyFrame(1f, 18f);
            shadow.StartAnimation(nameof(shadow.BlurRadius), shadowBlurAnimation);

            var shadowOffsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            shadowOffsetAnimation.Duration = TimeSpan.FromMilliseconds(durationHover);
            shadowOffsetAnimation.InsertKeyFrame(1f, new Vector3(shadowDistance, shadowDistance, 0));
            shadow.StartAnimation(nameof(Visual.Offset), shadowOffsetAnimation);

            var sprite = _compositor.CreateSpriteVisual();
            sprite.Size = new System.Numerics.Vector2((float)shadowBorder.ActualWidth, (float)shadowBorder.ActualHeight);
            sprite.Shadow = shadow;

            ElementCompositionPreview.SetElementChildVisual((UIElement)shadowBorder.FindName("Shadow"), sprite);

            //Move the spotlight to the center of the hovered light

            var ttv = frameworkElement.TransformToVisual(galleryPage);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            var mouse_x = screenCoords.X + frameworkElement.ActualWidth / 2;
            var mouse_y = screenCoords.Y + frameworkElement.ActualHeight / 2;


            var elementOffset = new Vector3((float)mouse_x, (float)mouse_y, spotlight_z); // EF: Need to calculate tile offset here
            var spotlightOffsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            spotlightOffsetAnimation.Duration = TimeSpan.FromMilliseconds(durationHover);
            spotlightOffsetAnimation.InsertKeyFrame(1f, elementOffset);
            spotLight.StartAnimation(nameof(Visual.Offset), spotlightOffsetAnimation);

            var ambientLightColorAnimation = _compositor.CreateColorKeyFrameAnimation();
            ambientLightColorAnimation.Duration = TimeSpan.FromMilliseconds(durationHover);
            ambientLightColorAnimation.InsertKeyFrame(1f, ambientLightColorOn);
            ambientLight.StartAnimation(nameof(AmbientLight.Color), ambientLightColorAnimation);
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

        #region lighting

        private void ApplyLighting()
        {
            ambientLight = _compositor.CreateAmbientLight();
            ambientLight.Color = ambientLightColorOff;

            spotLight = _compositor.CreateSpotLight();
            spotLight.CoordinateSpace = _gridview;
            spotLight.Offset = new Vector3(0, 0, spotlight_z);
            spotLight.InnerConeColor = Colors.Yellow;
            spotLight.InnerConeAngleInDegrees = 0.0f;
            spotLight.OuterConeColor = Colors.DodgerBlue;
            spotLight.OuterConeAngleInDegrees = 45.0f;
        }

        private void PhotoCollectionViewer_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            spotLight.Offset = new Vector3(5000, 5000, spotlight_z);

        }
        #endregion

    }
}