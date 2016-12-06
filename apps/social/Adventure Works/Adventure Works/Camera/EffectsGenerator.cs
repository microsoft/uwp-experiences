using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;

namespace Adventure_Works
{
    public class EffectsGenerator
    {
        private CanvasBitmap _canvasBitmap;
        private CanvasDevice _canvasDevice;
        private CanvasRenderTarget _canvasRenderer;

        private Rect _imgRect;

        private EffectsGenerator()
        {

        }

        public static async Task<EffectsGenerator> LoadImage(IRandomAccessStream stream)
        {
            EffectsGenerator ef = new EffectsGenerator();

            ef._canvasDevice = new CanvasDevice();
            ef._canvasBitmap = await CanvasBitmap.LoadAsync(ef._canvasDevice, stream);

            var imgRect = new Rect();
            var width = ef._canvasBitmap.Size.Width;
            var height = ef._canvasBitmap.Size.Height;

            if (width > height)
            {
                imgRect.Width = Math.Round(width * 150 / height);
                imgRect.Height = 150;
                imgRect.X = -((imgRect.Width - imgRect.Height) / 2);
                imgRect.Y = 0;
            }
            else if (height > width)
            {
                imgRect.Width = 150;
                imgRect.Height = Math.Round(height * 150 / width);
                imgRect.X = 0;
                imgRect.Y = -((imgRect.Height - imgRect.Width) / 2);
            }
            else
            {
                imgRect.Width = 150;
                imgRect.Height = 150;
                imgRect.X = 0;
                imgRect.Y = 0;
            }

            ef._imgRect = imgRect;

            ef._canvasRenderer = new CanvasRenderTarget(ef._canvasDevice, 150, 150, ef._canvasBitmap.Dpi);

            return ef;
        }

        public async Task<IRandomAccessStream> GenerateImageWithEffect(EffectType effectType)
        {
            using (var ds = _canvasRenderer.CreateDrawingSession())
            {
                ds.Clear(Colors.Black);
                ds.DrawImageWithEffect(_canvasBitmap, _imgRect, new Rect(0, 0, _canvasBitmap.Size.Width, _canvasBitmap.Size.Height), effectType);
            }

            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();

            await _canvasRenderer.SaveAsync(stream, CanvasBitmapFileFormat.Png);

            return stream;
        }
    }

    public static class EffectDraw
    {
        public static void DrawImageWithEffect(this CanvasDrawingSession ds, ICanvasImage canvasImage, Rect destinationRect, Rect sourceRect, EffectType effectType)
        {
            ICanvasImage effect = canvasImage;

            switch (effectType)
            {
                case EffectType.none:
                    effect = canvasImage;
                    break;
                case EffectType.amet:
                    effect = CreateGrayscaleEffect(canvasImage);
                    break;
                case EffectType.lorem:
                    effect = CreateColorMatrixEffect(canvasImage, 253.7515f);
                    break;
                case EffectType.oratio:
                    effect = CreateColorMatrixEffect(canvasImage, 35.03401f);
                    break;
                case EffectType.primis:
                    effect = CreateColorMatrixEffect(canvasImage, 568.929932f);
                    break;
                case EffectType.erant:
                    effect = CreateColorMatrixEffect(canvasImage, 595.475159f); 
                    break;
                case EffectType.suas:
                    effect = CreateColorMatrixEffect(canvasImage, 685.6845f);
                    break;
                case EffectType.ipsum:
                    effect = CreateConvolveMatrixEffect(canvasImage);
                    break;
                case EffectType.dolor:
                    effect = CreateEdgeDetectionEffect(canvasImage);
                    break;
                case EffectType.denique:
                    effect = CreateDarkerEffect(canvasImage);
                    break;
                case EffectType.atqui:
                    effect = CreateBrighterEffect(canvasImage);
                    break;
                case EffectType.veniam:
                    effect = CreateDirectionalBlureEffect(canvasImage);
                    break;
                case EffectType.unum:
                    effect = CreateDiscreteTranferEffect(canvasImage);
                    break;
                case EffectType.exerci:
                    effect = CreateInvertEffect(canvasImage);
                    break;
            }

            ds.DrawImage(effect, destinationRect, sourceRect);
        }

        private static ICanvasImage CreateGrayscaleEffect(ICanvasImage canvasImage)
        {
            var ef = new GrayscaleEffect();
            ef.Source = canvasImage;
            return ef;
        }

        private static ICanvasImage CreateColorMatrixEffect(ICanvasImage canvasImage, float coeff)
        {
            var ef = new ColorMatrixEffect
            {
                Source = canvasImage
            };

            var matrix = new Matrix5x4();

            matrix.M11 = (float)Math.Sin(coeff * 1.5);
            matrix.M21 = (float)Math.Sin(coeff * 1.4);
            matrix.M31 = (float)Math.Sin(coeff * 1.3);
            matrix.M51 = (1 - matrix.M11 - matrix.M21 - matrix.M31) / 2;

            matrix.M12 = (float)Math.Sin(coeff * 1.2);
            matrix.M22 = (float)Math.Sin(coeff * 1.1);
            matrix.M32 = (float)Math.Sin(coeff * 1.0);
            matrix.M52 = (1 - matrix.M12 - matrix.M22 - matrix.M32) / 2;

            matrix.M13 = (float)Math.Sin(coeff * 0.9);
            matrix.M23 = (float)Math.Sin(coeff * 0.8);
            matrix.M33 = (float)Math.Sin(coeff * 0.7);
            matrix.M53 = (1 - matrix.M13 - matrix.M23 - matrix.M33) / 2;

            matrix.M44 = 1;

            ef.ColorMatrix = matrix;

            return ef;
        }

        private static ICanvasImage CreateConvolveMatrixEffect(ICanvasImage canvasImage)
        {
            var ef = new ConvolveMatrixEffect
            {
                Source = canvasImage,
                KernelWidth = 3,
                KernelHeight = 3,
                KernelMatrix = new float[] {-0.0821647644f, 0.917835236f, 0, 0.917835236f, -3.794588f, 1, 0, 1, 0.0821647644f }
            };

            return ef;
        }

        private static ICanvasImage CreateEdgeDetectionEffect(ICanvasImage canvasImage)
        {
            var ef = new EdgeDetectionEffect
            {
                Source = canvasImage,
                Amount = 0.9828464f,
                BlurAmount = 0.232849464f
            };

            return ef;
        }

        private static ICanvasImage CreateDarkerEffect(ICanvasImage canvasImage)
        {
            var ef = new BrightnessEffect
            {
                Source = canvasImage,
                BlackPoint = new System.Numerics.Vector2(0.27f, 0f),
                WhitePoint = new System.Numerics.Vector2(0.85f)
            };

            return ef;
        }

        private static ICanvasImage CreateBrighterEffect(ICanvasImage canvasImage)
        {
            var ef = new BrightnessEffect
            {
                Source = canvasImage,
                BlackPoint = new System.Numerics.Vector2(0.01f, 0.2f),
                WhitePoint = new System.Numerics.Vector2(0.85f, 0.95f)
            };

            return ef;
        }

        private static ICanvasImage CreateDirectionalBlureEffect(ICanvasImage canvasImage)
        {
            var ef = new DirectionalBlurEffect
            {
                Source = canvasImage,
                Angle = 31.7106724f,
                BlurAmount = 24.8950157f
            };

            return ef;
        }

        private static ICanvasImage CreateDiscreteTranferEffect(ICanvasImage canvasImage)
        {
            var ef = new DiscreteTransferEffect
            {
                Source = canvasImage,
                RedTable = new float[] { 1, 0 },
                GreenTable = new float[] { 1, 0 },
                BlueTable = new float[] { 0, 1 },
            };

            return ef;
        }

        private static ICanvasImage CreateInvertEffect(ICanvasImage canvasImage)
        {
            var ef = new InvertEffect
            {
                Source = canvasImage
            };

            return ef;
        }
    }

    public enum EffectType
    {
        none,
        amet,
        denique,
        atqui,
        exerci,
        dolor,
        lorem,
        oratio,
        primis,
        erant,
        suas,
        veniam,
        unum,
        ipsum,

    }
}
