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
using Windows.System;

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
        private float incrementDirection = 1.0f;

        public ImageEditingPage()
        {
            InitializeComponent();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
        }

        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            var key = args.VirtualKey;
            if (key == VirtualKey.Control)
            {
                incrementDirection = 1.0f;
            }
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            var key = args.VirtualKey;
            if (key == VirtualKey.Add)
            {
                IncrementValue(0.1f);
            }
            else if (key == VirtualKey.Subtract)
            {
                IncrementValue(-0.1f);
            }
            else if (key == VirtualKey.Control)
            {
                incrementDirection = -1.0f;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _photo = e.Parameter as Photo;
            _compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;
            _propertySet = _compositor.CreatePropertySet();
            _propertySet.InsertVector3("Contrast", new Vector3());
            _propertySet.InsertVector3("Saturation", new Vector3(2f, 2f, 2f));
            _propertySet.InsertVector3("Exposure", new Vector3());
            _propertySet.InsertVector3("Blur", new Vector3());

            SetupRingAnimation();
            SetupAnimation();
            SetupDialControl();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (RadialController.IsSupported())
            {
                _radialConfiguration.IsMenuSuppressed = false;

                _radialController.RotationChanged -= RadialController_RotationChanged;
                _radialController.ScreenContactStarted -= RadialController_ScreenContactStarted;
                _radialController.ScreenContactEnded -= RadialController_ScreenContactEnded;
                _radialController.ButtonPressed -= RadialController_ButtonPressed;
                _radialController.ButtonReleased -= RadialController_ButtonReleased;
            }

            _effect.Dispose();
            _canvasBitmap.Dispose();
            _canvasRenderTarget.Dispose();
        }

        /// <summary>
        /// TODO: 3. Dial Control
        /// </summary>
        private void SetupDialControl()
        {
            if (RadialController.IsSupported())
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
                IncrementValue(increment);
            }
        }

        private void IncrementValue(float increment)
        {
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
                var newValue = _contrast + (value * incrementDirection);
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
                var newValue = _saturation + (value * incrementDirection);
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
                var newValue = _blur + (value * incrementDirection);
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
                var newValue = _exposure + (value * incrementDirection);
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
            fadeInAnimation.DelayTime = TimeSpan.FromMilliseconds(250);
            fadeInAnimation.Duration = TimeSpan.FromMilliseconds(750);

            var fadeOutAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeOutAnimation.InsertKeyFrame(0f, 1f);
            fadeOutAnimation.InsertKeyFrame(1f, 0f);
            fadeOutAnimation.Target = "Opacity";
            fadeOutAnimation.Duration = TimeSpan.FromMilliseconds(1000);

            var offsetInAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetInAnimation.InsertKeyFrame(0f, new Vector3(0f, -50f, 0f));
            offsetInAnimation.InsertKeyFrame(1f, new Vector3());
            offsetInAnimation.Target = "Offset";
            offsetInAnimation.Duration = TimeSpan.FromMilliseconds(250);

            ElementCompositionPreview.SetImplicitShowAnimation(CanvasControl, fadeInAnimation);
            ElementCompositionPreview.SetImplicitShowAnimation(CanvasControl, offsetInAnimation);

            fadeInAnimation.Duration = TimeSpan.FromMilliseconds(1500);
            ElementCompositionPreview.SetImplicitShowAnimation(ContrastGrid, fadeInAnimation);
            ElementCompositionPreview.SetImplicitShowAnimation(ExposureGrid, fadeInAnimation);
            ElementCompositionPreview.SetImplicitShowAnimation(SaturationGrid, fadeInAnimation);
            ElementCompositionPreview.SetImplicitShowAnimation(BlurGrid, fadeInAnimation);
            ElementCompositionPreview.SetImplicitShowAnimation(TwitterLogo, fadeInAnimation);

            ElementCompositionPreview.SetImplicitHideAnimation(CanvasControl, fadeOutAnimation);
            ElementCompositionPreview.SetImplicitHideAnimation(ContrastGrid, fadeOutAnimation);
            ElementCompositionPreview.SetImplicitHideAnimation(ExposureGrid, fadeOutAnimation);
            ElementCompositionPreview.SetImplicitHideAnimation(SaturationGrid, fadeOutAnimation);
            ElementCompositionPreview.SetImplicitHideAnimation(BlurGrid, fadeOutAnimation);
            ElementCompositionPreview.SetImplicitHideAnimation(TwitterLogo, fadeOutAnimation);
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
                ds.DrawImage(_canvasBitmap, new Rect(new Point((Window.Current.Bounds.Width - _canvasBitmap.SizeInPixels.Width) / 2, 0), _canvasBitmap.Size));
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
                var properties = e.GetCurrentPoint(this).Properties;
                if (properties.IsLeftButtonPressed)
                {
                    Contrast = 0.1f;
                }
                else if (properties.IsRightButtonPressed)
                {
                    Contrast = -0.1f;
                }
                CreateEffects();
            }
        }

        private void ExposureChanged(object sender, PointerRoutedEventArgs e)
        {
            if (CanvasControl.ReadyToDraw)
            {
                var properties = e.GetCurrentPoint(this).Properties;
                if (properties.IsLeftButtonPressed)
                {
                    Exposure = 0.1f;
                }
                else if (properties.IsRightButtonPressed)
                {
                    Exposure = -0.1f;
                }
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
                var properties = e.GetCurrentPoint(this).Properties;
                if (properties.IsLeftButtonPressed)
                {
                    Saturation = -0.1f;
                }
                else if (properties.IsRightButtonPressed)
                {
                    Saturation = +0.1f;
                }
                CreateEffects();
            }
        }

        private void BlurChanged(object sender, PointerRoutedEventArgs e)
        {
            if (CanvasControl.ReadyToDraw)
            {
                var properties = e.GetCurrentPoint(this).Properties;
                if (properties.IsLeftButtonPressed)
                {
                    Blur = 10;
                }
                else if (properties.IsRightButtonPressed)
                {
                    Blur = -10;
                }
                CreateEffects();
            }
        }

        #endregion
    }
}