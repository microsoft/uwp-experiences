using GHIElectronics.UWP.Shields;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HeartRateDevice
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private FEZHAT _fezhat;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private DispatcherTimer _sampleTimer = new DispatcherTimer();

        private bool _buttonPressed = false;
        private int _currentHeartRate = 0;
        private DateTime previousBeat;

        private bool _isPhysicalDevice = false;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT")
            {
                _isPhysicalDevice = await InitPhysicalDevice();

                if (_isPhysicalDevice)
                {
                    HardwareDevice.Visibility = Visibility.Visible;
                    SoftwareDevice.Visibility = Visibility.Collapsed;
                }
            }

            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        private async Task<bool> InitPhysicalDevice()
        {
            _fezhat = await FEZHAT.CreateAsync();
            if (_fezhat != null)
            {
                _fezhat.DIO24On = true;

                _sampleTimer.Interval = TimeSpan.FromMilliseconds(50);
                _sampleTimer.Tick += SampleTimer_Tick;
                _sampleTimer.Start();

                return true;
            }

            return false;
        }

        private void SampleTimer_Tick(object sender, object e)
        {
            if (_fezhat != null)
            {
                // KeyUp
                if (!_fezhat.IsDIO22Pressed() && _buttonPressed)
                {
                    _buttonPressed = false;

                    var now = DateTime.Now;
                    if (previousBeat != default(DateTime))
                    {
                        var t = now - previousBeat;

                        _currentHeartRate = (int)(60000 / t.TotalMilliseconds);
                        CurrentHeartRateText.Text = _currentHeartRate.ToString();
                    }

                    previousBeat = now;
                    _fezhat.D2.TurnOff();
                }
                // KeyDown
                else if (_fezhat.IsDIO22Pressed() && !_buttonPressed)
                {
                    _buttonPressed = true;
                    _fezhat.D2.Color = new FEZHAT.Color(255, 0, 0);
                }
            }

            Debug.WriteLine("current heartrate = " + _currentHeartRate);
        }

        private async void DispatcherTimer_Tick(object sender, object e)
        {
            // follow the steps here to connect the project to Azure IoT Hub = https://github.com/Microsoft/AppDevXbox/tree/BestForYou_iot_app
            await AzureIoTHub.SendDeviceToCloudMessageAsync(_isPhysicalDevice ? _currentHeartRate : new Random().Next(-3, 3) + (int)HeartRateSlider.Value);
        }
    }
}
