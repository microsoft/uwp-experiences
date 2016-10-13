using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Yoga.Models;
using Microsoft.Toolkit.Uwp.UI.Animations;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Yoga.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrainingPage : Page
    {
        public TrainingPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Videos = e.Parameter as Video;
        }

        public Video Videos { get; set; }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            ListView listView = this.CompletedListView;

            var tag = ((FrameworkElement)sender).Tag;

            switch (tag.ToString())
            {
                case "Completed":
                    listView = this.CompletedListView;
                    break;
                case "Browse":
                    listView = this.BrowseListView;
                    break;
                case "Suggested":
                    listView = this.SuggestedListView;
                    break;
            }

            var listWidth = listView.ActualWidth;
            var farRightItemPosition = 0d;
            int? leftItemIndex = null;
            int? lastItemIndex = null;

            for (int i = 0; i < listView.Items.Count; i++)
            {
                var container = (FrameworkElement)listView.ContainerFromIndex(i);
                if (container == null) { continue; }
                var point = container.TransformToVisual(this).TransformPoint(default(Point));
                farRightItemPosition = point.X + container.ActualWidth;

                if (point.X + container.ActualWidth >= 0 && !leftItemIndex.HasValue)
                {
                    leftItemIndex = i;
                }

                if (farRightItemPosition > listWidth)
                {
                    if (!lastItemIndex.HasValue)
                    {
                        lastItemIndex = i - 1;
                    }

                    int index;

                    if (lastItemIndex.Value == leftItemIndex.Value)
                    {
                        index = ++i;
                    }
                    else
                    {
                        index = i + (lastItemIndex.Value - leftItemIndex.Value);
                    }

                    object item;
                    if (index < listView.Items.Count)
                    {
                        item = listView.Items[index];
                    }
                    else
                    {
                        item = listView.Items.Last();
                    }

                    listView.ScrollIntoView(item);
                    break;
                }
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            ListView listView = this.CompletedListView;

            var tag = ((FrameworkElement)sender).Tag;

            switch (tag.ToString())
            {
                case "Completed":
                    listView = this.CompletedListView;
                    break;
                case "Browse":
                    listView = this.BrowseListView;
                    break;
                case "Suggested":
                    listView = this.SuggestedListView;
                    break;
            }

            var listWidth = listView.ActualWidth;
            var farRightItemPosition = 0d;
            int? leftItemIndex = null;
            int? lastItemIndex = null;

            for (int i = 0; i < listView.Items.Count; i++)
            {
                var container = (FrameworkElement)listView.ContainerFromIndex(i);
                if (container == null) { continue; }
                var point = container.TransformToVisual(this).TransformPoint(default(Point));
                farRightItemPosition = point.X + container.ActualWidth;

                if (point.X + container.ActualWidth >= 0 && !leftItemIndex.HasValue)
                {
                    leftItemIndex = i;
                }

                if (farRightItemPosition > listWidth)
                {
                    if (!lastItemIndex.HasValue)
                    {
                        lastItemIndex = i - 1;
                    }

                    int index;

                    if (lastItemIndex.Value == leftItemIndex.Value)
                    {
                        index = --i;
                    }
                    else
                    {
                        index = leftItemIndex.Value - (lastItemIndex.Value - leftItemIndex.Value);
                    }

                    object item;
                    if (index > 0)
                    {
                        item = listView.Items[index];
                    }
                    else
                    {
                        item = listView.Items.First();
                    }

                    listView.ScrollIntoView(item);
                    break;
                }
            }
        }

        private VideoDialog dialog = new VideoDialog();

        private async  void ListView_ItemClicked(object sender, ItemClickEventArgs e)
        {
            var addedItem = e.ClickedItem as TrainingVideo;
            dialog.Title = addedItem.Name;
            dialog.ElementSoundMode = ElementSoundMode.Off;
            dialog.MinWidth = Window.Current.Bounds.Width * 0.8;
            dialog.MinHeight = Window.Current.Bounds.Height * 0.8;
            await dialog.ShowAsync();
        }
    }
}
