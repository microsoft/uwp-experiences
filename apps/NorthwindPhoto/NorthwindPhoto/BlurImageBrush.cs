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
using System.Linq;
using Windows.Graphics.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;

namespace NorthwindPhoto
{
    public class BlurImageBrush : XamlCompositionBrushBase
    {
        private LoadedImageSurface _surface;
        public CompositionPropertySet PropertySet;

        /// <summary>
        /// </summary>
        protected override void OnConnected()
        {
            var compositor = Window.Current.Compositor;

            // Load milky way image
            _surface = LoadedImageSurface.StartLoadFromUri(new Uri(App.PhotoCollection.ElementAt(4).Path));

            _surface.LoadCompleted += (s, startArgs) =>
            {
                if (startArgs.Status == LoadedImageSourceLoadStatus.Success)
                {
                    // Create blur effect
                    var brush = compositor.CreateSurfaceBrush(_surface);
                    brush.Stretch = CompositionStretch.UniformToFill;

                    IGraphicsEffect graphicsEffect = new GaussianBlurEffect
                    {
                        Name = "Blur",
                        BlurAmount = 0,
                        Source = new CompositionEffectSourceParameter("image")
                    };

                    var effectFactory = compositor.CreateEffectFactory(graphicsEffect, new[] {"Blur.BlurAmount"});
                    var effectBrush = effectFactory.CreateBrush();
                    effectBrush.SetSourceParameter("image", brush);

                    // Composition Brush is what is being applied to the UI Element.
                    CompositionBrush = effectBrush;
                    PropertySet = effectBrush.Properties;

                    // Animate the blur
                    var blurAnimation = compositor.CreateScalarKeyFrameAnimation();
                    blurAnimation.InsertKeyFrame(0, 0f);
                    blurAnimation.InsertKeyFrame(0.5f, 10.0f);
                    blurAnimation.InsertKeyFrame(1, 0);
                    blurAnimation.Duration = TimeSpan.FromSeconds(4);
                    blurAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

                    effectBrush.Properties.StartAnimation("Blur.BlurAmount", blurAnimation);
                }
            };
        }

        protected override void OnDisconnected()
        {
            if (_surface != null)
            {
                _surface.Dispose();
                _surface = null;
            }

            if (CompositionBrush != null)
            {
                CompositionBrush.Dispose();
                CompositionBrush = null;
            }
        }
    }
}