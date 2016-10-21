using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Adventure_Works
{
    public class ImageExFromFilePath : ImageEx
    {


        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(ImageExFromFilePath), new PropertyMetadata(default(string), OnFilePathChanged));

        private static void OnFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
            {
                return;
            }

            var control = d as ImageExFromFilePath;
            var path = e.NewValue as string;

            var bi = new BitmapImage();
            control.Source = bi;

            var t = Task.Run(async () =>
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(path);
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
        }
    }
}
