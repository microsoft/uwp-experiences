using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Adventure_Works.Ink
{
    public static class InkHelpers
    {
        public async static Task<string> SaveInkToFile(this InkCanvas inker, string filename)
        {
            if (inker == null || inker.InkPresenter.StrokeContainer.GetStrokes().Count == 0)
            {
                return null;
            }

            var file = await (await AdventureObjectStorageHelper.GetDataSaveFolder()).CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            if (file == null)
                return null;

            using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await inker.InkPresenter.StrokeContainer.SaveAsync(outputStream);
            }

            return file.Path;
        }

        public async static Task LoadInkFromFile(this InkCanvas inker, string filename)
        {
            if (filename == null)
            {
                return;
            }

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(filename);
                if (file != null)
                {
                    using (var stream = await file.OpenReadAsync())
                    {
                        inker.InkPresenter.StrokeContainer.Clear();
                        await inker.InkPresenter.StrokeContainer.LoadAsync(stream);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public static async Task<IRandomAccessStream> RenderImageWithInkToStreamAsync(InkCanvas inker, IRandomAccessStream imageStream)
        {
            if (inker.InkPresenter.StrokeContainer.GetStrokes().Count == 0)
            {
                return null;
            }

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)inker.ActualWidth, (int)inker.ActualHeight, 96);

            var image = await CanvasBitmap.LoadAsync(device, imageStream);
            using (var ds = renderTarget.CreateDrawingSession())
            {
                var imageBounds = image.GetBounds(device);
                var min = Math.Min(imageBounds.Height, imageBounds.Width);

                imageBounds.X = (imageBounds.Width - min) / 2;
                imageBounds.Y = (imageBounds.Height - min) / 2;
                imageBounds.Height = min;
                imageBounds.Width = min;

                ds.Clear(Colors.White);
                ds.DrawImage(image, new Rect(0, 0, inker.ActualWidth, inker.ActualWidth), imageBounds);
                ds.DrawInk(inker.InkPresenter.StrokeContainer.GetStrokes());
            }
            var stream = new InMemoryRandomAccessStream();
            await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Jpeg, 1f);

            return stream;
        }

        public static async Task<string> RenderImageWithInkToFileAsync(InkCanvas inker, IRandomAccessStream imageStream, string filename)
        {
            if (inker.InkPresenter.StrokeContainer.GetStrokes().Count == 0)
            {
                return null;
            }

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)inker.ActualWidth, (int)inker.ActualHeight, 96);

            var image = await CanvasBitmap.LoadAsync(device, imageStream);
            using (var ds = renderTarget.CreateDrawingSession())
            {
                var imageBounds = image.GetBounds(device);
                var min = Math.Min(imageBounds.Height, imageBounds.Width);

                imageBounds.X = (imageBounds.Width - min) / 2;
                imageBounds.Y = (imageBounds.Height - min) / 2;
                imageBounds.Height = min;
                imageBounds.Width = min;

                ds.Clear(Colors.White);
                ds.DrawImage(image, new Rect(0, 0, inker.ActualWidth, inker.ActualWidth), imageBounds);
                ds.DrawInk(inker.InkPresenter.StrokeContainer.GetStrokes());
            }

            var file = await (await AdventureObjectStorageHelper.GetDataSaveFolder()).CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Jpeg, 1f);
            }

            return file.Path;

        }
    }
}
