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
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NorthwindPhoto.Model;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace NorthwindPhoto
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageEditingPage : Page
    {
        private const double _preciseMovement = 10;
        private const double _normalMovement = 30;
        private const float _outerRing = 200f;
        private float _blur, _contrast, _saturation = 1, _exposure;
        private CanvasBitmap _canvasBitmap;
        private CanvasRenderTarget _canvasRenderTarget;
        private Compositor _compositor;

        private ICanvasEffect _effect;
        private Photo _photo;
        private CompositionPropertySet _propertySet;
        private RadialControllerConfiguration _radialConfiguration;

        private RadialController _radialController;
        private string _selectedControl;

        public ImageEditingPage()
        {
            InitializeComponent();

            SetupRingAnimation();
            SetupAnimation();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _photo = e.Parameter as Photo;

            var image = new BitmapImage(new Uri(_photo.Path));

            image.ImageOpened += (sender, ev) =>
            {
                animationTarget.Opacity = 1;
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("Image");
                animation.Completed += (s, st) =>
                {
                    var item = FindName("CanvasControl");
                };
                animation.TryStart(animationTarget);
            };

            animationTarget.Opacity = 0;
            animationTarget.Source = image;

            _propertySet = _compositor.CreatePropertySet();
            _propertySet.InsertVector3("Contrast", new Vector3());
            _propertySet.InsertVector3("Saturation", new Vector3(2f, 2f, 2f));
            _propertySet.InsertVector3("Exposure", new Vector3());
            _propertySet.InsertVector3("Blur", new Vector3());


            SetupDialControl();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Canvas.SetZIndex(this, 1);
            var animation = _compositor.CreateScalarKeyFrameAnimation();
            animation.Target = "Opacity";
            animation.Duration = TimeSpan.FromSeconds(0.6);
            animation.InsertKeyFrame(1, 0);
            ElementCompositionPreview.SetImplicitHideAnimation(this, animation);


            animationTarget.Visibility = Visibility.Visible;
            animationTarget.Opacity = 1;
            var service = ConnectedAnimationService.GetForCurrentView();
            service.DefaultDuration = TimeSpan.FromSeconds(0.6);
            service.PrepareToAnimate("Image", animationTarget);
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _radialConfiguration.IsMenuSuppressed = false;

            _radialController.RotationChanged -= RadialController_RotationChanged;
            _radialController.ScreenContactStarted -= RadialController_ScreenContactStarted;
            _radialController.ScreenContactEnded -= RadialController_ScreenContactEnded;
            _radialController.ButtonPressed -= RadialController_ButtonPressed;
            _radialController.ButtonReleased -= RadialController_ButtonReleased;

            _effect?.Dispose();
            _canvasBitmap?.Dispose();
            _canvasRenderTarget?.Dispose();
        }

        /// <summary>
        /// TODO: 3. Dial Control
        /// </summary>
        private void SetupDialControl()
        {
            // Singleton instance for the app using RadialController.CreateForCurrentView();
            _radialController = App.RadialController;

            // Suppress the current menu from the dial
            _radialConfiguration = RadialControllerConfiguration.GetForCurrentView();
            _radialConfiguration.ActiveControllerWhenMenuIsSuppressed = App.RadialController;
            _radialConfiguration.IsMenuSuppressed = true;

            _radialController.RotationResolutionInDegrees = _normalMovement;
            _radialController.ButtonHolding += RadialController_ButtonHolding;
            _radialController.RotationChanged += RadialController_RotationChanged;
            _radialController.ScreenContactStarted += RadialController_ScreenContactStarted;
            _radialController.ScreenContactEnded += RadialController_ScreenContactEnded;
            _radialController.ButtonPressed += RadialController_ButtonPressed;
            _radialController.ButtonReleased += RadialController_ButtonReleased;
        }

        /// <summary>
        /// When the dial touches the screen, find which ring is being activated.
        /// </summary>
        private void RadialController_ScreenContactStarted(RadialController sender,
            RadialControllerScreenContactStartedEventArgs args)
        {
            var elements =
                VisualTreeHelper.FindElementsInHostCoordinates(args.Contact.Position, this).Cast<FrameworkElement>();

            // Each ring has a tag which helps us determine the photo manipulation
            _selectedControl = elements.FirstOrDefault(i => i.Tag != null).Tag.ToString();

            HideUnusedRings();
        }

        /// <summary>
        /// The dial is removed from the screen
        /// </summary>
        private void RadialController_ScreenContactEnded(RadialController sender, object args)
        {
            _selectedControl = null;
            ShowAllRings();
        }

        /// <summary>
        /// When dial is not pressed, change resolution to 30 degrees
        /// </summary>
        private void RadialController_ButtonReleased(RadialController sender,
            RadialControllerButtonReleasedEventArgs args)
        {
            _radialController.RotationResolutionInDegrees = _normalMovement;
        }

        /// <summary>
        /// When dial is pressed, change resolution to 5 degrees
        /// </summary>
        private void RadialController_ButtonPressed(RadialController sender, RadialControllerButtonPressedEventArgs args)
        {
            _radialController.RotationResolutionInDegrees = _preciseMovement;
        }

        /// <summary>
        /// When dial is held down, change resolution to 5 degrees
        /// </summary>
        private void RadialController_ButtonHolding(RadialController sender, RadialControllerButtonHoldingEventArgs args)
        {
            _radialController.RotationResolutionInDegrees = _preciseMovement;
        }

        /// <summary>
        /// When the dial is turned.
        /// </summary>
        private void RadialController_RotationChanged(RadialController sender,
            RadialControllerRotationChangedEventArgs args)
        {
            if (!string.IsNullOrEmpty(_selectedControl) && CanvasControl.ReadyToDraw)
            {
                // Check to see if the dial is pressed during rotation.
                var increment = CalculateIncrements(args);

                switch (_selectedControl)
                {
                    case "Contrast":
                        Contrast = +increment;
                        break;
                    case "Saturation":
                        Saturation = +increment;
                        break;
                    case "Exposure":
                        Exposure = +increment;
                        break;
                    case "Blur":
                        Blur = +increment * 10;
                        break;
                }

                CreateEffects();
            }
        }

        /// <summary>
        /// Calculate the increments based on delta and if the movement is normal or precise
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static float CalculateIncrements(RadialControllerRotationChangedEventArgs args)
        {
            float variance;
            if (Math.Abs(args.RotationDeltaInDegrees) == _normalMovement)
            {
                if (args.RotationDeltaInDegrees < 0)
                    variance = -0.1f;
                else
                    variance = 0.1f;
            }
            else
            {
                if (args.RotationDeltaInDegrees < 0)
                    variance = -0.01f;
                else
                    variance = 0.01f;
            }

            return variance;
        }

        #region Win2D helpers

        private async void Twitter_Clicked(object sender, PointerRoutedEventArgs e)
        {
            var twitterDialog = new TwitterDialog(CanvasControl, _canvasBitmap.Dpi, _effect);
            var result = await twitterDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
                await new MessageDialog("Tweet successful").ShowAsync();
        }

        public float Contrast
        {
            get { return _contrast; }
            set
            {
                var newValue = _contrast + value;
                if (newValue < 0)
                    newValue = 0;
                else if (newValue > 1)
                    newValue = 1;
                _propertySet.InsertVector3("Contrast", new Vector3(1 + newValue, 1 + newValue, 1 + newValue));
                _contrast = newValue;
            }
        }

        public float Saturation
        {
            get { return _saturation; }
            set
            {
                var newValue = _saturation + value;
                if (newValue < 0)
                    newValue = 0;
                else if (newValue > 1)
                    newValue = 1;

                _propertySet.InsertVector3("Saturation", new Vector3(1 + newValue, 1 + newValue, 1 + newValue));
                _saturation = newValue;
            }
        }

        public float Blur
        {
            get { return _blur; }
            set
            {
                var newValue = _blur + value;
                if (newValue < 0)
                    newValue = 0;
                else if (newValue > 100)
                    newValue = 100;

                _propertySet.InsertVector3("Blur",
                    new Vector3(1 + newValue / 100, 1 + newValue / 100, 1 + newValue / 100));
                _blur = newValue;
            }
        }

        public float Exposure
        {
            get { return _exposure; }
            set
            {
                var newValue = _exposure + value;
                if (newValue < 0)
                    newValue = 0;
                else if (newValue > 1)
                    newValue = 1;

                _propertySet.InsertVector3("Exposure", new Vector3(1 + newValue, 1 + newValue, 1 + newValue));
                _exposure = newValue;
            }
        }

        private void SetupRingAnimation()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;

            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0f, new Vector3(1f, 1f, 1f));
            scaleAnimation.InsertKeyFrame(0.5f, new Vector3(1.25f, 1.25f, 1.25f));
            scaleAnimation.InsertKeyFrame(1f, new Vector3(1f, 1f, 1f));
            scaleAnimation.Duration = TimeSpan.FromSeconds(3);
            scaleAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

            var contrastRing = ElementCompositionPreview.GetElementVisual(ContrastOuterRing);
            contrastRing.CenterPoint = new Vector3(_outerRing / 2, _outerRing / 2, _outerRing / 2);
            contrastRing.StartAnimation("Scale", scaleAnimation);

            var saturationRing = ElementCompositionPreview.GetElementVisual(SaturationOuterRing);
            saturationRing.CenterPoint = new Vector3(_outerRing / 2, _outerRing / 2, _outerRing / 2);
            saturationRing.StartAnimation("Scale", scaleAnimation);

            var exposureRing = ElementCompositionPreview.GetElementVisual(ExposureOuterRing);
            exposureRing.CenterPoint = new Vector3(_outerRing / 2, _outerRing / 2, _outerRing / 2);
            exposureRing.StartAnimation("Scale", scaleAnimation);

            var blurRing = ElementCompositionPreview.GetElementVisual(BlurOuterRing);
            blurRing.CenterPoint = new Vector3(_outerRing / 2, _outerRing / 2, _outerRing / 2);
            blurRing.StartAnimation("Scale", scaleAnimation);
        }

        private void SetupAnimation()
        {
            var fadeInAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeInAnimation.InsertKeyFrame(0f, 0f);
            fadeInAnimation.InsertKeyFrame(1f, 1f);
            fadeInAnimation.Target = "Opacity";
            fadeInAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
            fadeInAnimation.DelayTime = TimeSpan.FromMilliseconds(600);
            fadeInAnimation.Duration = TimeSpan.FromMilliseconds(2000);

            var offsetInAnimation = _compositor.CreateScalarKeyFrameAnimation();
            offsetInAnimation.InsertKeyFrame(0f, 100);
            offsetInAnimation.InsertKeyFrame(1f, 0);
            offsetInAnimation.Target = "Translation.Y";
            offsetInAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
            offsetInAnimation.DelayTime = TimeSpan.FromMilliseconds(600);
            offsetInAnimation.Duration = TimeSpan.FromMilliseconds(2000);

            var inGroup = _compositor.CreateAnimationGroup();
            inGroup.Add(fadeInAnimation);
            inGroup.Add(offsetInAnimation);

            var offsetOutAnimation = _compositor.CreateScalarKeyFrameAnimation();
            offsetOutAnimation.InsertKeyFrame(0f, 0);
            offsetOutAnimation.InsertKeyFrame(1f, 100);
            offsetOutAnimation.Target = "Translation.Y";
            offsetOutAnimation.Duration = TimeSpan.FromMilliseconds(700);

            var fadeOutAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeOutAnimation.InsertKeyFrame(0f, 1f);
            fadeOutAnimation.InsertKeyFrame(1f, 0f);
            fadeOutAnimation.Target = "Opacity";
            fadeOutAnimation.Duration = TimeSpan.FromMilliseconds(600);

            var outGroup = _compositor.CreateAnimationGroup();
            outGroup.Add(fadeOutAnimation);
            outGroup.Add(offsetOutAnimation);

            ElementCompositionPreview.SetIsTranslationEnabled(ContrastGrid, true);
            ElementCompositionPreview.SetIsTranslationEnabled(ExposureGrid, true);
            ElementCompositionPreview.SetIsTranslationEnabled(SaturationGrid, true);
            ElementCompositionPreview.SetIsTranslationEnabled(BlurGrid, true);


            ElementCompositionPreview.SetImplicitShowAnimation(ContrastGrid, inGroup);
            ElementCompositionPreview.SetImplicitShowAnimation(ExposureGrid, inGroup);
            ElementCompositionPreview.SetImplicitShowAnimation(SaturationGrid, inGroup);
            ElementCompositionPreview.SetImplicitShowAnimation(BlurGrid, inGroup);
            ElementCompositionPreview.SetImplicitShowAnimation(TwitterLogo, fadeInAnimation);

            ElementCompositionPreview.SetImplicitHideAnimation(ContrastGrid, outGroup);
            ElementCompositionPreview.SetImplicitHideAnimation(ExposureGrid, outGroup);
            ElementCompositionPreview.SetImplicitHideAnimation(SaturationGrid, outGroup);
            ElementCompositionPreview.SetImplicitHideAnimation(BlurGrid, outGroup);
        }

        private void HideUnusedRings()
        {
            switch (_selectedControl)
            {
                case "Contrast":
                    ExposureGrid.Visibility = Visibility.Collapsed;
                    SaturationGrid.Visibility = Visibility.Collapsed;
                    BlurGrid.Visibility = Visibility.Collapsed;

                    var contrastRing = ElementCompositionPreview.GetElementVisual(ContrastOuterRing);
                    var contrastExpressionAnimation = _compositor.CreateExpressionAnimation("myPropertySet.Contrast");
                    contrastExpressionAnimation.SetReferenceParameter("myPropertySet", _propertySet);
                    contrastRing.StartAnimation("Scale", contrastExpressionAnimation);

                    break;
                case "Saturation":
                    ExposureGrid.Visibility = Visibility.Collapsed;
                    ContrastGrid.Visibility = Visibility.Collapsed;
                    BlurGrid.Visibility = Visibility.Collapsed;

                    var saturationRing = ElementCompositionPreview.GetElementVisual(SaturationOuterRing);
                    var saturationExpressionAnimation = _compositor.CreateExpressionAnimation("myPropertySet.Saturation");
                    saturationExpressionAnimation.SetReferenceParameter("myPropertySet", _propertySet);
                    saturationRing.StartAnimation("Scale", saturationExpressionAnimation);
                    break;
                case "Exposure":
                    ContrastGrid.Visibility = Visibility.Collapsed;
                    SaturationGrid.Visibility = Visibility.Collapsed;
                    BlurGrid.Visibility = Visibility.Collapsed;

                    var exposureRing = ElementCompositionPreview.GetElementVisual(ExposureOuterRing);
                    var exposureExpressionAnimation = _compositor.CreateExpressionAnimation("myPropertySet.Exposure");
                    exposureExpressionAnimation.SetReferenceParameter("myPropertySet", _propertySet);
                    exposureRing.StartAnimation("Scale", exposureExpressionAnimation);
                    break;
                case "Blur":
                    ExposureGrid.Visibility = Visibility.Collapsed;
                    SaturationGrid.Visibility = Visibility.Collapsed;
                    ContrastGrid.Visibility = Visibility.Collapsed;

                    var blurRing = ElementCompositionPreview.GetElementVisual(BlurOuterRing);
                    var blurExpressionAnimation = _compositor.CreateExpressionAnimation("myPropertySet.Blur");
                    blurExpressionAnimation.SetReferenceParameter("myPropertySet", _propertySet);
                    blurRing.StartAnimation("Scale", blurExpressionAnimation);
                    break;
            }
        }

        private void ShowAllRings()
        {
            ExposureGrid.Visibility = Visibility.Visible;
            SaturationGrid.Visibility = Visibility.Visible;
            BlurGrid.Visibility = Visibility.Visible;
            ContrastGrid.Visibility = Visibility.Visible;

            SetupRingAnimation();
        }

        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var drawingSession = args.DrawingSession;
            drawingSession.DrawImage(_effect);
            CanvasControl.Opacity = 1;
            animationTarget.Opacity = 0;
        }

        private async void CanvasControl_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        private async Task CreateResourcesAsync(CanvasControl sender)
        {
            _canvasBitmap = await CanvasBitmap.LoadAsync(sender, new Uri(_photo.Path));
            _canvasRenderTarget = new CanvasRenderTarget(sender, (float)Window.Current.Bounds.Width,
                (float)Window.Current.Bounds.Height, _canvasBitmap.Dpi);

            CreateEffects();
        }

        private void CreateEffects()
        {
            _canvasRenderTarget = new CanvasRenderTarget(CanvasControl, (float)Window.Current.Bounds.Width,
                (float)Window.Current.Bounds.Height, _canvasBitmap.Dpi);

            using (var ds = _canvasRenderTarget.CreateDrawingSession())
            {
                ds.DrawImage(_canvasBitmap, new Rect(new Point(0, 0), animationTarget.RenderSize));
            }

            var blur = new GaussianBlurEffect
            {
                BlurAmount = _blur,
                Source = _canvasRenderTarget
            };

            var exposure = new ExposureEffect
            {
                Exposure = _exposure,
                Source = blur
            };

            var saturation = new SaturationEffect
            {
                Saturation = _saturation,
                Source = exposure
            };

            var contrast = new ContrastEffect
            {
                Contrast = _contrast,
                Source = saturation
            };

            _effect = contrast;
            CanvasControl.Invalidate();
        }

        private void CanvasControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (CanvasControl.ReadyToDraw)
                CreateEffects();
        }

        private void ContrastChanged(object sender, PointerRoutedEventArgs e)
        {
            if (CanvasControl.ReadyToDraw)
            {
                Contrast = 0.1f;
                CreateEffects();
            }
        }

        private void ExposureChanged(object sender, PointerRoutedEventArgs e)
        {
            if (CanvasControl.ReadyToDraw)
            {
                Exposure = 0.1f;
                CreateEffects();
            }
        }

        private void RingGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _selectedControl = (sender as FrameworkElement).Tag.ToString();
            HideUnusedRings();
        }

        private void RingGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _selectedControl = null;
            ShowAllRings();
        }

        private void SaturationChanged(object sender, PointerRoutedEventArgs e)
        {
            if (CanvasControl.ReadyToDraw)
            {
                Saturation = -0.1f;
                CreateEffects();
            }
        }

        private void BlurChanged(object sender, PointerRoutedEventArgs e)
        {
            if (CanvasControl.ReadyToDraw)
            {
                Blur = 10;
                CreateEffects();
            }
        }

        #endregion
    }
}