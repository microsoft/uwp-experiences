using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace Adventure_Works
{
    public class WrapPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Size finalSize = new Size { Width = availableSize.Width };
            double x = 0, x_max = 0;

            double rowHeight = 0d;
            foreach (var child in Children)
            {
                child.Measure(availableSize);

                x += child.DesiredSize.Width;
                if (x > availableSize.Width)
                {
                    if (x - child.DesiredSize.Width > x_max)
                    {
                        x_max = x - child.DesiredSize.Width;
                    }
                    x = child.DesiredSize.Width;
                    finalSize.Height += rowHeight;
                    rowHeight = child.DesiredSize.Height;
                }
                else
                {
                    rowHeight = Math.Max(child.DesiredSize.Height, rowHeight);
                }
            }

            if (x > x_max)
            {
                x_max = x;
            }

            finalSize.Height += rowHeight;
            finalSize.Width = x_max;
            return finalSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect finalRect = new Rect(0, 0, finalSize.Width, finalSize.Height);

            double rowHeight = 0;
            foreach (var child in Children)
            {
                if ((child.DesiredSize.Width + finalRect.X) > finalSize.Width)
                {
                    finalRect.X = 0;
                    finalRect.Y += rowHeight;
                    rowHeight = 0;
                }

                child.Arrange(new Rect(finalRect.X, finalRect.Y, child.DesiredSize.Width, child.DesiredSize.Height));

                finalRect.X += child.DesiredSize.Width;
                rowHeight = Math.Max(child.DesiredSize.Height, rowHeight);
            }

            return finalSize;
        }
    }
}
