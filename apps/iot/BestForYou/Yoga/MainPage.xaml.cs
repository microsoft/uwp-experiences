using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Yoga.Models;
using Yoga.Pages;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Yoga
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Models/data.json"));
            var json = File.ReadAllText(storageFile.Path);
            YogaData = JsonConvert.DeserializeObject<YogaData>(json);

            StatsRadioButton.IsChecked = true;
        }

        public YogaData YogaData { get; set; }

        private void NavigationButton_Checked(object sender, RoutedEventArgs e)
        {
            var contentControl = sender as ContentControl;

            if (contentControl?.Content != null)
            {
                var value = contentControl.Content;

                switch (value.ToString())
                {
                    case "STATS":
                        MainFrame.Navigate(typeof(StatsPage), YogaData.Poses);
                        break;
                    case "TRAINING":
                        MainFrame.Navigate(typeof(TrainingPage), YogaData.Videos.First());
                        break;
                    case "GEAR":
                        MainFrame.Navigate(typeof(GearPage), YogaData.Videos);
                        break;
                }
            }
        }
    }
}
