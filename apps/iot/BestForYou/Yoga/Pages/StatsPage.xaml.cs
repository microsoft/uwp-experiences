using Microsoft.Toolkit.Uwp.UI.Animations;
using ppatierno.AzureSBLite;
using ppatierno.AzureSBLite.Messaging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Yoga.Models;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Yoga.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StatsPage : Page, INotifyPropertyChanged
    {
        private static string ConnectionString = $"Endpoint={Keys.EventHubCompatibleEndpoint};SharedAccessKeyName={Keys.IoTHubAccessPolicyName};SharedAccessKey={Keys.IoTHubAccessPolicyPrimaryKey}";
        private static string EventHubEntity = Keys.EventHubCompatibleName;
        private static string PartitionId = "1";

        private Compositor _compositor;

        public StatsPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;
            PoseData = new ObservableCollection<Pose>((Pose[])e.Parameter);

            GenerateTimings();

            this.SizeChanged += StatsPage_SizeChanged;

            await Task.Run(() =>
            {
                CheckHeartRate();
            });

            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            this.SizeChanged -= StatsPage_SizeChanged;
        }

        private double _pageWidth = Window.Current.Bounds.Width;
        private int barMargin = 5;

        private void StatsPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth < 720)
            {
                barMargin = 2;
            }
            else
            {
                barMargin = 5;
            }

            _pageWidth = this.ActualWidth - 96 - (barMargin * 31);

            foreach (var item in Graph.Items)
            {
                var itemContainer = (ListViewItem)Graph.ContainerFromItem(item);

                if (itemContainer == null)
                {
                    return;
                }

                itemContainer.MinWidth = (_pageWidth / 31) + barMargin;

                var bar = itemContainer.ContentTemplateRoot as FrameworkElement;
                bar.Width = (_pageWidth / 31);

                var sprite = ElementCompositionPreview.GetElementChildVisual(VisualTreeHelper.GetChild(bar, 0) as UIElement);
                sprite.Size = new System.Numerics.Vector2((float)bar.Width, sprite.Size.Y);
            }
        }

        public ObservableCollection<Pose> PoseData { get; set; }

        public ObservableCollection<WorkoutTiming> WorkoutTimings { get; set; } = new ObservableCollection<WorkoutTiming>();

        private void GenerateTimings()
        {
            var random = new Random((int)DateTime.Now.Ticks);
            for (int i = 1; i < 32; i++)
            {
                WorkoutTimings.Add(new WorkoutTiming { Day = i, Minutes = random.Next(90) });
            }
        }

        private void Graph_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var workoutTiming = args.Item as WorkoutTiming;

            args.ItemContainer.MinWidth = (_pageWidth / 31) + barMargin;

            var bar = args.ItemContainer.ContentTemplateRoot as FrameworkElement;
            bar.Width = (_pageWidth / 31);

            var sprite = _compositor.CreateSpriteVisual();
            sprite.Size = new System.Numerics.Vector2((float)bar.Width, (float)workoutTiming.Minutes);
            sprite.Offset = new System.Numerics.Vector3(0f, (float)(-workoutTiming.Minutes) - 4, 0f);
            sprite.CenterPoint = new System.Numerics.Vector3(0f, (float)(workoutTiming.Minutes) + 4, 0f);
            var solidColorBrush = Application.Current.Resources["HighlightColor"] as SolidColorBrush;
            sprite.Brush = _compositor.CreateColorBrush(solidColorBrush.Color);

            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            scaleAnimation.InsertKeyFrame(0f, new System.Numerics.Vector3(1f, 0f, 0f));
            scaleAnimation.InsertKeyFrame(1f, new System.Numerics.Vector3(1f, 1f, 1f));
            sprite.StartAnimation("Scale", scaleAnimation);

            ElementCompositionPreview.SetElementChildVisual(VisualTreeHelper.GetChild(bar, 0) as UIElement, sprite);
        }


        private string _heartRate;
        private DispatcherTimer _timer;

        public event PropertyChangedEventHandler PropertyChanged;

        public string HeartRate
        {
            get { return _heartRate; }
            set
            {
                _heartRate = value;
                if (_timer != null)
                {
                    var t = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        _timer.Interval = TimeSpan.FromMilliseconds(1000d / (double.Parse(value) / 60));
                        if (!_timer.IsEnabled)
                        {
                            _timer.Start();
                        }
                    });
                }
                RaisePropertyChanged();
            }
        }

        private async Task CheckHeartRate()
        {
            var factory = MessagingFactory.CreateFromConnectionString(ConnectionString);

            var client = factory.CreateEventHubClient(EventHubEntity);
            var group = client.GetDefaultConsumerGroup();

            var startingDateTimeUtc = DateTime.Now;

            var receiver = group.CreateReceiver(PartitionId, startingDateTimeUtc);

            while (true)
            {
                EventData data = receiver.Receive();
                if (data == null) continue;

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    HeartRate = Encoding.UTF8.GetString(data.GetBytes());
                });

                Debug.WriteLine("{0} {1} {2}", data.PartitionKey, data.EnqueuedTimeUtc.ToLocalTime(), HeartRate);
            }
        }

        private void _timer_Tick(object sender, object e)
        {
            HeartLogoFill.Fade(1, 50).Then().Fade(0).Start();
        }

        private void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_GotFocus(object sender, RoutedEventArgs e)
        {
            if (App.IsXbox())
            {
                Scroller.ChangeView(0, 0, 1, false);
            }
        }
    }
}