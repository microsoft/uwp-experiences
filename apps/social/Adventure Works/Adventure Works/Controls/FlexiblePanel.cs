using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace Adventure_Works
{
    public class FlexiblePanel : Panel
    {

        /// <summary>
        /// Gets or sets the desired size of each item. 
        /// If Orientation is horizontal, this value sets the desired height. Otherwise, this value sets the desired width.
        /// </summary>
        /// <value>
        /// The desired size of each item
        /// </value>
        public double ItemDesiredSize
        {
            get { return (double)GetValue(ItemDesiredSizeProperty); }
            set { SetValue(ItemDesiredSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemDesiredSize dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemDesiredSizeProperty =
            DependencyProperty.Register("ItemDesiredSize", typeof(double), typeof(FlexiblePanel), new PropertyMetadata(150d, PropertyChanged));


        /// <summary>
        /// Gets or sets the orientation in which child elements are aranged
        /// </summary>
        /// <value>
        /// The orientation
        /// </value>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the OrientationProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(FlexiblePanel), new PropertyMetadata(Orientation.Horizontal, PropertyChanged));

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FlexiblePanel).InvalidateMeasure();
            (d as FlexiblePanel).InvalidateArrange();
            (d as FlexiblePanel).UpdateLayout();
        }

        private Size MeasureOverideHorizontal(Size availableSize)
        {
            var resultSize = new Size(0, 0);

            if (!this.Children.Any())
            {
                return resultSize;
            }

            double x = 0;
            double y = 0;

            List<List<UIElement>> items = new List<List<UIElement>>();

            List<UIElement> currentRow = new List<UIElement>();

            for (var i = 0; i < Children.Count; ++i)
            {
                var curChild = Children[i];
                curChild.Measure(new Size(availableSize.Width, ItemDesiredSize));

                if (x + curChild.DesiredSize.Width > availableSize.Width)
                {
                    var widthDifWithChild = (x + curChild.DesiredSize.Width) - availableSize.Width;
                    var widthDifWithoutChild = availableSize.Width - x;

                    double scaleFactor = 1;
                    if (widthDifWithChild < widthDifWithoutChild)
                    {
                        //increase size to all children including curCHild
                        currentRow.Add(curChild);
                        scaleFactor = availableSize.Width / (x + curChild.DesiredSize.Width);
                        foreach (var item in currentRow)
                        {
                            item.Measure(new Size(Math.Floor(item.DesiredSize.Width * scaleFactor), Math.Floor(item.DesiredSize.Height * scaleFactor)));
                        }
                        currentRow.Clear();
                        y += curChild.DesiredSize.Height;
                        x = 0;
                    }
                    else
                    {
                        scaleFactor = availableSize.Width / x;
                        foreach (var item in currentRow)
                        {
                            item.Measure(new Size(Math.Floor(item.DesiredSize.Width * scaleFactor), Math.Floor(item.DesiredSize.Height * scaleFactor)));
                        }

                        y += currentRow.First().DesiredSize.Height;
                        currentRow.Clear();
                        currentRow.Add(curChild);
                        x = curChild.DesiredSize.Width;
                    }
                }
                else
                {
                    x += curChild.DesiredSize.Width;
                    currentRow.Add(curChild);
                }
            }

            if (currentRow.Count > 0)
            {
                var widthDif = availableSize.Width - x;
                if (widthDif < currentRow.Last().DesiredSize.Width / 2)
                {
                    var scaleFactor = availableSize.Width / x;

                    foreach (var item in currentRow)
                    {
                        item.Measure(new Size(Math.Floor(item.DesiredSize.Width * scaleFactor), Math.Floor(item.DesiredSize.Height * scaleFactor)));
                    }
                }

                y += currentRow.First().DesiredSize.Height;
            }

            resultSize.Width = availableSize.Width;
            resultSize.Height = y;

            return resultSize;
        }

        private Size MeasureOverideVertical(Size availableSize)
        {
            return availableSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Orientation == Orientation.Horizontal)
                return MeasureOverideHorizontal(availableSize);
            else
                return MeasureOverideVertical(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (!this.Children.Any())
            {
                return finalSize;
            }

            double y = 0;
            double x = 0;

            var previousChildHeight = 0d;

            foreach (var child in this.Children)
            {
                if (Math.Floor(x + child.DesiredSize.Width) > finalSize.Width)
                {
                    x = 0;
                    y += previousChildHeight;
                }

                child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
                x += child.DesiredSize.Width;
                previousChildHeight = child.DesiredSize.Height;
            }

            return new Size(finalSize.Width, y + previousChildHeight);
        }
    }
}