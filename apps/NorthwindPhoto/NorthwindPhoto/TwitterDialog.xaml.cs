// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Toolkit.Uwp.Services.Twitter;
using NorthwindPhoto.Model;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace NorthwindPhoto
{
    public sealed partial class TwitterDialog : ContentDialog
    {
        private readonly CanvasControl _canvasControl;
        private readonly float _dpi;
        private readonly ICanvasEffect _effectCanvas;

        private TwitterDialog()
        {
            InitializeComponent();
        }

        public TwitterDialog(CanvasControl canvasControl, float dpi, ICanvasEffect effectCanvas)
            : this()
        {
            _canvasControl = canvasControl;
            _dpi = dpi;
            _effectCanvas = effectCanvas;
        }

        /// <summary>
        /// TODO: 4. UWP Community Toolkit
        /// </summary>
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // For more information please visit. http://aka.ms/uwpcommunitytoolkit
            TwitterService.Instance.Initialize(Constants.ConsumerKey, Constants.ConsumerSecret, Constants.Callback);

            // If the credentials aren't stored then Twitter UI will appear for authorization
            if (!await TwitterService.Instance.LoginAsync())
                return;

            using (var stream = new InMemoryRandomAccessStream())
            {
                // Get the image from the Win2D Canvas
                await GetImageStreamAsync(stream);

                // Post a tweet with a picture
                await TwitterService.Instance.TweetStatusAsync(TweetTextBox.Text, stream);
            }

            Hide();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }

        private async Task GetImageStreamAsync(InMemoryRandomAccessStream stream)
        {
            var canvasRenderTarget = new CanvasRenderTarget(_canvasControl, (float) Window.Current.Bounds.Width,
                (float) Window.Current.Bounds.Height, _dpi);

            using (var ds = canvasRenderTarget.CreateDrawingSession())
            {
                ds.DrawImage(_effectCanvas);
            }

            await canvasRenderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Jpeg, 0.5f);
        }
    }
}