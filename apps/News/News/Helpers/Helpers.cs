using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace News.Helpers
{
    public static class Helpers
    {
        public static IEnumerable<T> FindChildren<T>(this DependencyObject parent)
               where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null)
            {
                yield break;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // If the child is not of the request child type child
                var childType = child as T;

                if (childType != null)
                {
                    yield return childType;
                }

                foreach (var grandChild in FindChildren<T>(child))
                {
                    yield return grandChild;
                }
            }
        }
    }
}
