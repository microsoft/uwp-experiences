using Adventure_Works.Data;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Adventure_Works
{
    public class ImageExFromPhotoData : ImageEx
    {
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var image = GetTemplateChild("Image") as Image;
            if (image != null)
            {
                image.HorizontalAlignment = HorizontalAlignment.Center;
                image.VerticalAlignment = VerticalAlignment.Center;
            }
        }

        public PhotoData PhotoData
        {
            get { return (PhotoData)GetValue(PhotoDataProperty); }
            set { SetValue(PhotoDataProperty, value); }
        }

        public static readonly DependencyProperty PhotoDataProperty =
            DependencyProperty.Register("PhotoData", typeof(PhotoData), typeof(ImageExFromPhotoData), new PropertyMetadata(null, OnPhotoDataChanged));

        private static void OnPhotoDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue || e.NewValue == null)
            {
                return;
            }

            var control = d as ImageExFromPhotoData;
            var photo = e.NewValue as PhotoData;
            if (photo.ThumbnailUri != null)
            {
                var bi = new BitmapImage();
                control.Source = bi;

                var t = Task.Run(async () =>
                {
                    try
                    {
                        var file = await StorageFile.GetFileFromPathAsync(photo.ThumbnailUri);
                        var stream = await file.OpenReadAsync();

                        var t2 = control.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            bi.SetSourceAsync(stream);
                        });

                    }
                    catch (Exception ex)
                    {

                    }
                });

                control.Height = 228;
                control.Width = 180;
            }
            else if (photo.IsLocal)
            {
                var bi = new BitmapImage();
                control.Source = bi;

                var t = Task.Run(async () =>
                {
                    try
                    {
                        var file = await StorageFile.GetFileFromPathAsync(photo.Uri);
                        var stream = await file.OpenReadAsync();

                        var t2 = control.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            bi.SetSourceAsync(stream);
                        });

                    }
                    catch (Exception ex)
                    {

                    }
                });

                control.Height = 180;
                control.Width = 180;
            }
            else
            {
                var bi = new BitmapImage(new Uri(photo.Uri));
                control.Source = bi;

                control.Height = 180;
                control.Width = 180;
            }
        }
    }
}
