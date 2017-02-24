using Adventure_Works.CognitiveServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Adventure_Works
{
    public class Camera : IDisposable
    {
        // Receive notifications about rotation of the device and UI and apply any necessary rotation to the preview stream and UI controls       
        private readonly DisplayInformation _displayInformation = DisplayInformation.GetForCurrentView();
        private readonly SimpleOrientationSensor _orientationSensor = SimpleOrientationSensor.GetDefault();
        private SimpleOrientation _deviceOrientation = SimpleOrientation.NotRotated;
        private DisplayOrientations _displayOrientation = DisplayOrientations.Portrait;

        // Rotation metadata to apply to the preview stream and recorded videos (MF_MT_VIDEO_ROTATION)
        // Reference: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        private CaptureElement _captureElement;
        private MediaCapture _mediaCapture;
        private DeviceInformation _cameraDevice;
        private bool _initialized;

        private FaceDetectionEffect _faceDetectionEffect;
        private bool _analyzingEmotion = false;

        private bool _isSmileDetectionEnabled = false;

        public bool IsSmileDetectionEnabled
        {
            get { return _isSmileDetectionEnabled; }
            set
            {
                _isSmileDetectionEnabled = value;
                var t =_captureElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    SmileDetectionChanged?.Invoke(this, _isSmileDetectionEnabled);
                });
            }
        }


        // need to throtle checks to Cognitive Services for free tier
        private DateTime _lastEmotionCheck = default(DateTime);

        public event EventHandler<PictureCapturedEventArgs> PictureCaptured;
        public event EventHandler<bool> SmileDetectionChanged;

        public Camera (CaptureElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element", "element can't be null");
            }

            _captureElement = element;

            _displayOrientation = _displayInformation.CurrentOrientation;
            _displayInformation.OrientationChanged += DisplayInformation_OrientationChanged;

            if (_orientationSensor != null)
            {
                _deviceOrientation = _orientationSensor.GetCurrentOrientation();
                _orientationSensor.OrientationChanged += OrientationSensor_OrientationChanged;
            }

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.CameraPressed += HardwareButtons_CameraPressed;
            }

            Application.Current.Suspending += Application_Suspending;
            Application.Current.Resuming += ApplicationResuming;
            Window.Current.VisibilityChanged += Current_VisibilityChanged;
        }

        

        public async Task Initialize()
        {
            if (_cameraDevice == null)
            {
                var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                var desiredDevice = allVideoDevices.FirstOrDefault(device => device.EnclosureLocation != null && device.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);
                var cameraDevice = desiredDevice ?? allVideoDevices.FirstOrDefault();
                _cameraDevice = cameraDevice;
            }

            await InitializeWithCameraDevice(_cameraDevice);
        }

        private async Task InitializeWithCameraDevice(DeviceInformation cameraDevice)
        {
            if (_mediaCapture == null)
            {
                _cameraDevice = cameraDevice;
                _mediaCapture = new MediaCapture();

                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = _cameraDevice.Id };

                try
                {
                    await _mediaCapture.InitializeAsync(settings);

                    var b = MediaCapture.IsVideoProfileSupported(_cameraDevice.Id);

                    // get highest resolution for preview
                    var resolutions = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo).Select(x => x as VideoEncodingProperties);
                    var resolution = resolutions.Where(r => r != null).OrderByDescending(r => r.Height * r.Width).FirstOrDefault();

                    if (resolution != null)
                    {
                        await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, resolution);
                    }
                }
                catch
                {
                    _mediaCapture.Dispose();
                    _mediaCapture = null;
                    return;
                }

                if (!App.IsXbox() && (_cameraDevice.EnclosureLocation == null || _cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front))
                {
                    _captureElement.FlowDirection = FlowDirection.RightToLeft;
                }
                else
                {
                    _captureElement.FlowDirection = FlowDirection.LeftToRight;
                }

                _captureElement.Source = _mediaCapture;

                await _mediaCapture.StartPreviewAsync();

                await SetPreviewRotationAsync();

                var definition = new FaceDetectionEffectDefinition();
               // definition.SynchronousDetectionEnabled = false;
               // definition.DetectionMode = FaceDetectionMode.HighPerformance;

                _faceDetectionEffect = (await _mediaCapture.AddVideoEffectAsync(definition, MediaStreamType.VideoPreview)) as FaceDetectionEffect;
                _faceDetectionEffect.FaceDetected += FaceDetectionEffect_FaceDetected;

                _faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(100);
                _faceDetectionEffect.Enabled = true;

                _initialized = true;
            }
        }

        public Task StopAsync()
        {
            return CleanUp();
        }

        private async void FaceDetectionEffect_FaceDetected(FaceDetectionEffect sender, FaceDetectedEventArgs args)
        {
            if (args.ResultFrame.DetectedFaces.Count > 0 &&
                _isSmileDetectionEnabled &&
                !_analyzingEmotion && 
                (DateTime.Now - _lastEmotionCheck).TotalSeconds > 1)
            {
                _analyzingEmotion = true;

                var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
                double scale = 480d / (double)previewProperties.Height;

                VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)(previewProperties.Width * scale) , 480);

                using (var frame = await _mediaCapture.GetPreviewFrameAsync(videoFrame))
                {
                    if (frame.SoftwareBitmap != null)
                    {
                        var bitmap = frame.SoftwareBitmap;

                        InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                        encoder.SetSoftwareBitmap(bitmap);

                        await encoder.FlushAsync();

                        var smiling = await EmotionAPI.Instance.CheckIfEveryoneIsSmiling(stream, args.ResultFrame.DetectedFaces, scale);

                        _lastEmotionCheck = DateTime.Now;

                        if (smiling)
                        {
                            IsSmileDetectionEnabled = false;
                            await _captureElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => { await this.CapturePhotoAsync(); });
                            
                        }
                    }
                }
                
                _analyzingEmotion = false;
            }
        }

        public async Task ToggleCameraDeviceAsync()
        {
            if (_mediaCapture == null)
            {
                return;
            }

            await CleanUp();

            var allVideoDevices = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).ToList();
            int index = 0;

            if (_cameraDevice != null)
            {
                var currentDeviceIndex = allVideoDevices.IndexOf(allVideoDevices.Where(d => d.Id == _cameraDevice.Id).FirstOrDefault());
                if (currentDeviceIndex > -1)
                {
                    index = (currentDeviceIndex + 1) % allVideoDevices.Count;
                }
            }

            var cameraDevice = allVideoDevices[index];
            await InitializeWithCameraDevice(cameraDevice);
        }

        public async Task<bool> CanToggleCameraAsync()
        {
            var allVideoDevices = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).ToList();
            return allVideoDevices.Count > 1;
        }

        public async Task<StorageFile> CapturePhotoAsync()
        {
            if (_mediaCapture == null)
            {
                return null;
            }

            var stream = new InMemoryRandomAccessStream();

            await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

            try
            {
                var filename = "adventure_" + DateTime.Now.ToString("HHmmss_MMddyyyy") + ".jpg";

                var file = await (await AdventureObjectStorageHelper.GetDataSaveFolder()).CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);

                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                    var photoOrientation = ConvertOrientationToPhotoOrientation(GetCameraOrientation());
                    var properties = new BitmapPropertySet { { "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, Windows.Foundation.PropertyType.UInt16) } };

                    await encoder.BitmapProperties.SetPropertiesAsync(properties);
                    await encoder.FlushAsync();
                }

                stream.Dispose();
                PictureCaptured?.Invoke(this, new PictureCapturedEventArgs() { File = file });
                return file;
            }
            catch (Exception ex)
            {
                stream.Dispose();
                return null;
            }
        }

        private async Task CleanUp()
        {
            if (_faceDetectionEffect != null)
            {
                _faceDetectionEffect.FaceDetected -= FaceDetectionEffect_FaceDetected;
                _faceDetectionEffect.Enabled = false;

                await _mediaCapture?.RemoveEffectAsync(_faceDetectionEffect);

                _faceDetectionEffect = null;
            }

            if (_orientationSensor != null)
            {
                _orientationSensor.OrientationChanged -= OrientationSensor_OrientationChanged;
            }

            if (_displayInformation != null)
            {
                _displayInformation.OrientationChanged -= DisplayInformation_OrientationChanged;
            }

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.CameraPressed -= HardwareButtons_CameraPressed;
            }

            if (_mediaCapture != null)
            {
                if (_mediaCapture.CameraStreamState == Windows.Media.Devices.CameraStreamState.Streaming)
                {
                   await _mediaCapture.StopPreviewAsync();
                }

                _mediaCapture.Dispose();
                _mediaCapture = null;
            }

            _initialized = false;
        }

        /// <summary>
        /// Gets the current orientation of the UI in relation to the device (when AutoRotationPreferences cannot be honored) and applies a corrective rotation to the preview
        /// </summary>
        private async Task SetPreviewRotationAsync()
        {
            if (_mediaCapture == null || _mediaCapture.CameraStreamState != Windows.Media.Devices.CameraStreamState.Streaming)
                return;

            // Only need to update the orientation if the camera is mounted on the device
            if (_cameraDevice.EnclosureLocation == null || _cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Unknown) return;

            // Calculate which way and how far to rotate the preview
            int rotationDegrees = ConvertDisplayOrientationToDegrees(_displayOrientation);

            // The rotation direction needs to be inverted if the preview is being mirrored
            if (!App.IsXbox() && (_cameraDevice.EnclosureLocation == null || _cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front))
            {
                rotationDegrees = (360 - rotationDegrees) % 360;
            }

            // Add rotation metadata to the preview stream to make sure the aspect ratio / dimensions match when rendering and getting preview frames
            var props = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(RotationKey, rotationDegrees);
            await _mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
        }
        private async void ApplicationResuming(object sender, object e)
        {
            if (_initialized)
            {
                await Initialize();
            }
        }

        private async void Application_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            if (_initialized)
            {
                await CleanUp();
                _initialized = true;
            }

            deferral.Complete();
        }

        private async void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            if (e.Visible && _initialized)
            {
                await Initialize();
            }
            else if (!e.Visible && _initialized)
            {
                await CleanUp();
                _initialized = true;
            }
        }

        private async void HardwareButtons_CameraPressed(object sender, CameraEventArgs e)
        {
            await CapturePhotoAsync();
        }

        private async void DisplayInformation_OrientationChanged(DisplayInformation sender, object args)
        {
            _displayOrientation = sender.CurrentOrientation;
            await SetPreviewRotationAsync();
        }

        private void OrientationSensor_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            if (args.Orientation != SimpleOrientation.Faceup && args.Orientation != SimpleOrientation.Facedown)
            {
                // Only update the current orientation if the device is not parallel to the ground. This allows users to take pictures of documents (FaceUp)
                // or the ceiling (FaceDown) in portrait or landscape, by first holding the device in the desired orientation, and then pointing the camera
                // either up or down, at the desired subject.
                //Note: This assumes that the camera is either facing the same way as the screen, or the opposite way. For devices with cameras mounted
                //      on other panels, this logic should be adjusted.
                _deviceOrientation = args.Orientation;
            }
        }

        /// <summary>
        /// Calculates the current camera orientation from the device orientation by taking into account whether the camera is external or facing the user
        /// </summary>
        /// <returns>The camera orientation in space, with an inverted rotation in the case the camera is mounted on the device and is facing the user</returns>
        private SimpleOrientation GetCameraOrientation()
        {
            if (_cameraDevice.EnclosureLocation == null || _cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Unknown)
            {
                // Cameras that are not attached to the device do not rotate along with it, so apply no rotation
                return SimpleOrientation.NotRotated;
            }

            var result = _deviceOrientation;

            // Account for the fact that, on portrait-first devices, the camera sensor is mounted at a 90 degree offset to the native orientation
            if (_displayInformation.NativeOrientation == DisplayOrientations.Portrait)
            {
                switch (result)
                {
                    case SimpleOrientation.Rotated90DegreesCounterclockwise:
                        result = SimpleOrientation.NotRotated;
                        break;
                    case SimpleOrientation.Rotated180DegreesCounterclockwise:
                        result = SimpleOrientation.Rotated90DegreesCounterclockwise;
                        break;
                    case SimpleOrientation.Rotated270DegreesCounterclockwise:
                        result = SimpleOrientation.Rotated180DegreesCounterclockwise;
                        break;
                    case SimpleOrientation.NotRotated:
                        result = SimpleOrientation.Rotated270DegreesCounterclockwise;
                        break;
                }
            }

            // If the preview is being mirrored for a front-facing camera, then the rotation should be inverted
            if (!App.IsXbox() && (_cameraDevice.EnclosureLocation == null || _cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front))
            {
                // This only affects the 90 and 270 degree cases, because rotating 0 and 180 degrees is the same clockwise and counter-clockwise
                switch (result)
                {
                    case SimpleOrientation.Rotated90DegreesCounterclockwise:
                        return SimpleOrientation.Rotated270DegreesCounterclockwise;
                    case SimpleOrientation.Rotated270DegreesCounterclockwise:
                        return SimpleOrientation.Rotated90DegreesCounterclockwise;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the given orientation of the device in space to the corresponding rotation in degrees
        /// </summary>
        /// <param name="orientation">The orientation of the device in space</param>
        /// <returns>An orientation in degrees</returns>
        private static int ConvertDeviceOrientationToDegrees(SimpleOrientation orientation)
        {
            switch (orientation)
            {
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    return 90;
                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                    return 180;
                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    return 270;
                case SimpleOrientation.NotRotated:
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Converts the given orientation of the app on the screen to the corresponding rotation in degrees
        /// </summary>
        /// <param name="orientation">The orientation of the app on the screen</param>
        /// <returns>An orientation in degrees</returns>
        private static int ConvertDisplayOrientationToDegrees(DisplayOrientations orientation)
        {
            switch (orientation)
            {
                case DisplayOrientations.Portrait:
                    return 90;
                case DisplayOrientations.LandscapeFlipped:
                    return 180;
                case DisplayOrientations.PortraitFlipped:
                    return 270;
                case DisplayOrientations.Landscape:
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Converts the given orientation of the device in space to the metadata that can be added to captured photos
        /// </summary>
        /// <param name="orientation">The orientation of the device in space</param>
        /// <returns></returns>
        private static PhotoOrientation ConvertOrientationToPhotoOrientation(SimpleOrientation orientation)
        {
            switch (orientation)
            {
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    return PhotoOrientation.Rotate90;
                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                    return PhotoOrientation.Rotate180;
                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    return PhotoOrientation.Rotate270;
                case SimpleOrientation.NotRotated:
                default:
                    return PhotoOrientation.Normal;
            }
        }

        public void Dispose()
        {
            Application.Current.Suspending -= Application_Suspending;
            Application.Current.Resuming -= ApplicationResuming;

            CleanUp();
        }
    }
}
