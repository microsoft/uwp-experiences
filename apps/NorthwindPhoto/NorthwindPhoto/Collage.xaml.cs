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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace NorthwindPhoto
{
    /// <summary>
    /// TODO: 7 XAML Edit and Continue
    /// </summary>
    public sealed partial class Collage : Page
    {
        private Compositor _compositor;
        private ScalarKeyFrameAnimation _fadeIn, _fadeOut;
        private readonly List<Image> _images = new List<Image>();
        private readonly InkAnalyzer _inkAnalyzer;

        private InkPresenter _inkPresenter;
        private RadialControllerConfiguration _radialConfiguration;

        /// <summary>
        /// TODO: 5. Ink Shape Recognition
        /// </summary>
        public Collage()
        {
            InitializeComponent();

            _inkAnalyzer = new InkAnalyzer();
            SizeChanged += Collage_SizeChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;

            // User the events on the ink presenter
            _inkPresenter = MyInkCanvas.InkPresenter;
            _inkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen |
                                             CoreInputDeviceTypes.Touch;
            _inkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            _inkPresenter.StrokesErased += InkPresenter_StrokesErased;
            _inkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
            _inkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;

            // Radial controller used for recognition
            if (RadialController.IsSupported())
            {
                App.RadialController.ButtonPressed += RadialController_ButtonPressed;

                // Supress menu of controller
                _radialConfiguration = RadialControllerConfiguration.GetForCurrentView();
                _radialConfiguration.ActiveControllerWhenMenuIsSuppressed = App.RadialController;
                _radialConfiguration.IsMenuSuppressed = true;
            }

            // When using Cortana perform app protocol recognition
            App.ProtocolSubject.Subscribe(dataEventArgs => App_Protocol(dataEventArgs));

            ImageAnimationSetup();
        }

        /// <summary>
        /// When strokes are collected, add them to the analyzer for recognition
        /// </summary>
        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            _inkAnalyzer.AddDataForStrokes(args.Strokes);
        }

        /// <summary>
        /// When the user presses the controller analyze the strokes
        /// </summary>
        private async void RadialController_ButtonPressed(RadialController sender,
            RadialControllerButtonPressedEventArgs args)
        {
            await AnalyzeInkAsync();
        }

        /// <summary>
        /// Analyzes all the strokes collected from the ink presenter
        /// </summary>
        private async Task AnalyzeInkAsync()
        {
            // Does all the recognition
            var result = await _inkAnalyzer.AnalyzeAsync();

            if (result.Status == InkAnalysisStatus.Updated)
            {
                // Filter recognition by shapes. Options inlcude lists, paragraphs, words etc.
                var drawings = _inkAnalyzer.AnalysisRoot.FindNodes(InkAnalysisNodeKind.InkDrawing);

                foreach (InkAnalysisInkDrawing drawing in drawings)
                    if (drawing.DrawingKind == InkAnalysisDrawingKind.Rectangle
                        || drawing.DrawingKind == InkAnalysisDrawingKind.Square)
                    {
                        // Swap strokes with border, using details from drawing
                        var border = new Border
                        {
                            Width = drawing.BoundingRect.Width,
                            Height = drawing.BoundingRect.Height,
                            Margin = new Thickness(drawing.BoundingRect.X, drawing.BoundingRect.Y, 0, 0),
                            BorderThickness = new Thickness(new Random((int) DateTime.Now.Ticks).Next(5, 30)),
                            BorderBrush = new SolidColorBrush(Colors.Black),
                            Background = new SolidColorBrush(Colors.WhiteSmoke),
                            IsHitTestVisible = false
                        };

                        // Using the same composition show hide animation technique
                        ElementCompositionPreview.SetImplicitShowAnimation(border, _fadeIn);
                        ElementCompositionPreview.SetImplicitHideAnimation(border, _fadeOut);

                        Canvas.SetZIndex(border, -1);

                        // Image placeholder
                        var image = new Image
                        {
                            Width = drawing.BoundingRect.Width,
                            Height = drawing.BoundingRect.Height,
                            Margin = new Thickness(drawing.BoundingRect.X, drawing.BoundingRect.Y, 0, 0),
                            Stretch = Stretch.UniformToFill,
                            IsHitTestVisible = false,
                            Visibility = Visibility.Collapsed
                        };

                        ElementCompositionPreview.SetImplicitShowAnimation(image, _fadeIn);
                        _images.Add(image);

                        LayoutGrid.Children.Add(image);
                        LayoutGrid.Children.Add(border);
                    }

                // Same as shapes, we filter by words
                var inkWords = _inkAnalyzer.AnalysisRoot.FindNodes(InkAnalysisNodeKind.InkWord);
                foreach (InkAnalysisInkWord inkWord in inkWords)
                {
                    var textBlock = new TextBlock
                    {
                        Text = inkWord.RecognizedText,
                        Margin = new Thickness(inkWord.BoundingRect.X, inkWord.BoundingRect.Y, 0, 0),
                        FontSize = 128,
                        IsHitTestVisible = false
                    };

                    LayoutGrid.Children.Add(textBlock);
                }

                // Remove all strokes from canvas and analyzer.
                _inkAnalyzer.ClearDataForAllStrokes();
                _inkPresenter.StrokeContainer.Clear();
            }
        }


        /// <summary>
        /// Cortana calls or any protocol activation calls are handled by this method
        /// </summary>
        private void App_Protocol(ProtocolDataEventArgs e)
        {
            if (e.Image.ToLower() == "all")
            {
                var random = new Random();
                _randomUsedList.Clear();

                foreach (var image in _images)
                {
                    var next = -1;

                    while (next < 0)
                    {
                        var generatedIndex = random.Next(App.PhotoCollection.Count);
                        if (!_randomUsedList.Contains(generatedIndex))
                        {
                            next = generatedIndex;
                            _randomUsedList.Add(next);
                        }
                    }

                    image.Source = new BitmapImage(new Uri(App.PhotoCollection[next].Path));
                    image.Visibility = Visibility.Visible;
                }

                foreach (var border in LayoutGrid.Children.Where(i => i.GetType() == typeof(Border)))
                    LayoutGrid.Children.Remove(border);
            }
            else
            {
                var leftImage = _images.OrderBy(i => i.Margin.Left).FirstOrDefault();

                if (leftImage != null)
                    leftImage.Source = new BitmapImage(new Uri(e.Image));
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await ToastManager.CreateProgressToastAsync("MyMasterPiece.png", "Saving");
        }

        #region Helpers

        private void ImageAnimationSetup()
        {
            _fadeIn = _compositor.CreateScalarKeyFrameAnimation();
            _fadeIn.Target = "Opacity";
            _fadeIn.InsertKeyFrame(0f, 0f);
            _fadeIn.InsertKeyFrame(1f, 1f);
            _fadeIn.Duration = TimeSpan.FromSeconds(1);
            _fadeIn.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;

            _fadeOut = _compositor.CreateScalarKeyFrameAnimation();
            _fadeOut.Target = "Opacity";
            _fadeOut.InsertKeyFrame(1f, 0f);
            _fadeOut.Duration = TimeSpan.FromSeconds(3);
            _fadeOut.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
        }

        /// <summary>
        /// Analysis is done when the user right clicks on the mouse.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void UnprocessedInput_PointerPressed(InkUnprocessedInput sender, PointerEventArgs args)
        {
            await AnalyzeInkAsync();
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            _inkAnalyzer.RemoveDataForStrokes(args.Strokes.Select(i => i.Id));
        }

        private void Collage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MyInkCanvas.Width = Window.Current.CoreWindow.Bounds.Width;
            MyInkCanvas.Height = Window.Current.CoreWindow.Bounds.Height;
        }

        private readonly List<int> _randomUsedList = new List<int>();

        #endregion
    }
}