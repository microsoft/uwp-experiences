using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Controls
{
    [TemplatePart(Name = PartInker, Type = typeof(InkCanvas))]
    [TemplatePart(Name = PartRoot, Type = typeof(Grid))]
    [TemplatePart(Name = PartContent, Type = typeof(ContentPresenter))]
    public class InkOverlay : ContentControl
    {
        private const string PartInker = "Inker";
        private const string PartRoot = "Root";
        private const string PartContent = "Content";

        private InkCanvas _inker;
        private Grid _root;
        private ContentPresenter _contentPresenter;

        private DispatcherTimer _timer;
        private bool InkerActive;

        public InkOverlay()
        {
            DefaultStyleKey = typeof(InkOverlay);
        }

        protected override void OnApplyTemplate()
        {
            if (_inker != null)
            {
                _inker.InkPresenter.StrokesCollected -= InkPresenter_StrokesCollected;
            }

            if (_root != null)
            {
                _root.PointerEntered -= Root_PointerEntered;
                _root.PointerMoved -= Root_PointerMoved;
            }

            _inker = GetTemplateChild(PartInker) as InkCanvas;
            _root = GetTemplateChild(PartRoot) as Grid;
            _contentPresenter = GetTemplateChild(PartContent) as ContentPresenter;

            if (_inker != null && _root != null)
            {
                _inker.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen;
                var drawingAttributes = new InkDrawingAttributes
                {
                    DrawAsHighlighter = false,
                    Color = Colors.DarkBlue,
                    PenTip = PenTipShape.Circle,
                    IgnorePressure = false,
                    FitToCurve = true,
                    Size = new Size(3, 3)
                };

                _root.PointerEntered += Root_PointerEntered;
                _root.PointerMoved += Root_PointerMoved;

                _inker.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
                _inker.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;

                // fixes issue with long texts where ink is recognized before input is complete?
                // inker.InkPresenter.StrokeInput.StrokeStarted += (s, e) => { timer.Stop(); };

                InkerActive = false;
            }

            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Tick += Timer_Tick;
                _timer.Interval = TimeSpan.FromSeconds(2);
            }
            else
            {
                _timer.Stop();
            }

            base.OnApplyTemplate();
        }

        private void DecideInputMethod(Pointer pointer)
        {
            if (pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                if (!InkerActive)
                {
                    // enable Inker
                    InkerActive = true;
                    _inker.Visibility = Visibility.Visible;
                }
            }
            else if (InkerActive)
            {
                // disable Inker
                InkerActive = false;
                _timer.Stop();
                var t = RecognizeInkerText();
                _inker.Visibility = Visibility.Collapsed;
            }
        }

        private async Task RecognizeInkerText()
        {

            try
            {
                var inkRecognizer = new InkRecognizerContainer();
                var recognitionResults = await inkRecognizer.RecognizeAsync(_inker.InkPresenter.StrokeContainer, InkRecognitionTarget.All);

                List<TextBox> boxes = new List<TextBox>();

                var ttv = _contentPresenter.TransformToVisual(Window.Current.Content);
                Point offsetCoords = ttv.TransformPoint(new Point(0, 0));
                foreach (var result in recognitionResults)
                {
                    List<UIElement> elements = new List<UIElement>(
                        VisualTreeHelper.FindElementsInHostCoordinates(
                            new Rect(
                                new Point(
                                    result.BoundingRect.X + offsetCoords.X,
                                    result.BoundingRect.Y + offsetCoords.Y),
                            new Size(result.BoundingRect.Width, result.BoundingRect.Height)),
                        _contentPresenter));

                    // find one with overlap
                    var textBoxes = elements.Where(el => el is TextBox && (el as TextBox).IsEnabled);

                    var maxOverlapArea = 0d;
                    TextBox box = null;

                    foreach (var textBox in textBoxes)
                    {
                        var bounds = textBox.TransformToVisual(_contentPresenter).TransformBounds(new Rect(0, 0, textBox.RenderSize.Width, textBox.RenderSize.Height));
                        var xOverlap = Math.Max(0, Math.Min(result.BoundingRect.Right, bounds.Right) - Math.Max(result.BoundingRect.Left, bounds.Left));
                        var yOverlap = Math.Max(0, Math.Min(result.BoundingRect.Bottom, bounds.Bottom) - Math.Max(result.BoundingRect.Top, bounds.Top));
                        var overlapArea = xOverlap * yOverlap;

                        if (overlapArea > maxOverlapArea)
                        {
                            maxOverlapArea = overlapArea;
                            box = textBox as TextBox;
                        }
                    }

                    //TextBox box = elements.Where(el => el is TextBox && (el as TextBox).IsEnabled).FirstOrDefault() as TextBox;

                    if (box != null)
                    {
                        var text = result.GetTextCandidates().FirstOrDefault().Trim();

                        if (!boxes.Contains(box))
                        {
                            boxes.Add(box);

                            box.Text = text;
                        }
                        else
                        {
                            box.Text += " " + text;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
            }
            finally
            {
                _inker.InkPresenter.StrokeContainer.Clear();
            }
        }

        private string convertToDigits(string text)
        {
            return text.Replace('|', '1')
                       .Replace('-', '.')
                       .Replace('(', '1')
                       .Replace(')', '1');
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            _timer.Start();
        }

        private void Root_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            DecideInputMethod(e.Pointer);
        }

        private void Root_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            DecideInputMethod(e.Pointer);
        }

        private void Timer_Tick(object sender, object e)
        {
            _timer.Stop();
            var t = RecognizeInkerText();
        }
    }
}
