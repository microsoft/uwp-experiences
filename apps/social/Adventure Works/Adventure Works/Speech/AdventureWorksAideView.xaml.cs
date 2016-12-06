using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Adventure_Works
{
    public sealed partial class AdventureWorksAideView : UserControl
    {
        public AdventureWorksAideView()
        {
            this.InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    ListeningText.Visibility = Visibility.Visible;
                    TextTextBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ListeningText.Visibility = Visibility.Collapsed;
                    TextTextBlock.Visibility = Visibility.Visible;
                }

                TextTextBlock.Text = value;
                SetValue(TextProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AdventureWorksAideView), new PropertyMetadata(0));


        public AdventureWorksAideState State
        {
            get { return (AdventureWorksAideState)GetValue(StateProperty); }
            set {
                switch (value)
                {
                    case AdventureWorksAideState.Listening:
                        TextTextBlock.Foreground = App.Current.Resources["MainBackground"] as SolidColorBrush;
                        break;
                    case AdventureWorksAideState.Thinking:
                        TextTextBlock.Foreground = App.Current.Resources["BrandColor"] as SolidColorBrush;
                        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            while (State == AdventureWorksAideState.Thinking)
                            {
                                await Logo.Scale(1.3f, 1.3f, 20, 20, 200).StartAsync();
                                await Logo.Scale(1f, 1f, 20, 20, 600).StartAsync();
                            }
                        });
                        break;
                    case AdventureWorksAideState.Speaking:
                        TextTextBlock.Foreground = App.Current.Resources["BrandColor"] as SolidColorBrush;
                        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            while (State == AdventureWorksAideState.Speaking)
                            {
                                await Logo.Scale(1.3f, 1.3f, 20, 20, 200).StartAsync();
                                await Logo.Scale(1f, 1f, 20, 20, 200).StartAsync();
                            }
                        });
                        break;
                }
                SetValue(StateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for State.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(AdventureWorksAideState), typeof(AdventureWorksAideView), new PropertyMetadata(0));


        public Task Show()
        {
            var height = (float)this.ActualHeight / 2;
            var width = (float)this.ActualWidth / 2;

            var t1 = this.Scale(1.2f, 1.2f, width, height, 0)
                         .Then().Scale(1, 1, width, height, 200).Fade(1, 200).StartAsync();

            var t2 = Logo.Scale(0.2f, 0.2f, 20, 20, 0)
                         .Then().Scale(1, 1, 20, 20, 400).Rotate(720, 20, 20).StartAsync();

            return Task.WhenAll(t1, t2);
        }

        public Task Hide()
        {
            var height = (float)this.ActualHeight / 2;
            var width = (float)this.ActualWidth / 2;

            var t1 = this.Fade(0, 200).Scale(0.8f, 0.8f, width, height, 200).SetDelayForAll(300).StartAsync();
            var t2 = Logo.Scale(0,2f, 0.2f, 20, 20, 400).Rotate(0, 20, 20).StartAsync();

            return Task.WhenAll(t1, t2);
        }
    }
}
