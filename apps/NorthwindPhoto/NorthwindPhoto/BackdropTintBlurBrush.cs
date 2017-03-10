using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;

namespace NorthwindPhoto
{
    class BackdropTintBlurBrush : XamlCompositionBrushBase
    {
        protected override void OnConnected()
        {
            Compositor compositor = Window.Current.Compositor;

            // CompositionCapabilities: Are Effects supported?
            bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsSupported();
            FallbackColor = Color.FromArgb(100, 100, 100, 100);

            if (usingFallback)
            {
                // If Effects are not supported, use Fallback Solid Color
                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                return;
            }

            // Define Effect graph
            var graphicsEffect = new BlendEffect
                {
                    Mode = BlendEffectMode.SoftLight,
                    Background = new ColorSourceEffect()
                    {
                        Name = "Tint",
                        Color = Color.FromArgb(180, 255, 255, 255),
                    },
                    Foreground = new GaussianBlurEffect()
                    {
                        Name = "Blur",
                        Source = new CompositionEffectSourceParameter("Backdrop"),
                        BlurAmount = 10,
                        BorderMode = EffectBorderMode.Hard,
                    }
                };

            // Create EffectFactory and EffectBrush
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect);
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

            // Create BackdropBrush
            CompositionBackdropBrush backdrop = compositor.CreateBackdropBrush();
            effectBrush.SetSourceParameter("Backdrop", backdrop);

            // Set EffectBrush as the brush that XamlCompBrushBase paints onto Xaml UIElement
            CompositionBrush = effectBrush;      
        }

        protected override void OnDisconnected()
        {
            // Dispose CompositionBrushes if XamlCompBrushBase is removed from tree
            if (CompositionBrush != null)
            {
                CompositionBrush.Dispose();
                CompositionBrush = null;
            }
        }
    }
}
