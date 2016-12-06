using System;
using Adventure_Works.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Input.Inking;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.IO;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Adventure_Works
{
    public sealed partial class SlideshowSlideView : UserControl
    {
        public event EventHandler InkChanged;

        public PhotoData Photo
        {
            get { return (PhotoData)GetValue(PhotoProperty); }
            set { SetValue(PhotoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Photo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PhotoProperty =
            DependencyProperty.Register("Photo", typeof(PhotoData), typeof(SlideshowSlideView), new PropertyMetadata(null, OnPhotoChanged));

        private static void OnPhotoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as SlideshowSlideView;
            var photo = e.NewValue as PhotoData;

            view.Image.Source = new BitmapImage(new Uri(photo.Uri));
            view.Inker.InkPresenter.StrokeContainer.Clear();
        }

        public SlideshowSlideView()
        {
            this.InitializeComponent();

            UpdateSize();

            var drawingAttributes = new InkDrawingAttributes
            {
                DrawAsHighlighter = false,
                PenTip = PenTipShape.Circle,
                Size = new Size(4, 3),
                Color = Colors.Red
            };

            Inker.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            Inker.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen;
            Inker.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            Inker.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
        }

        private void UpdateSize()
        {
            if (Window.Current.Bounds.Height < 600 || Window.Current.Bounds.Width < 600)
            {
                Height = Width = 250;
            }
            else
            {
                Height = Width = 400;
            }
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            InkChanged?.Invoke(this, null);
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            InkChanged?.Invoke(this, null);
        }

        public async Task<byte[]> GetStrokeData()
        {
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await Inker.InkPresenter.StrokeContainer.SaveAsync(stream);
                stream.Seek(0);
                byte[] data = new byte[stream.Size];
                await stream.ReadAsync(data.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
                return data;
            }
        }

        public async Task UpdateStrokes(byte[] strokeData)
        {
            await Inker.InkPresenter.StrokeContainer.LoadAsync(strokeData.AsBuffer().AsStream().AsInputStream());
        }
    }
}
