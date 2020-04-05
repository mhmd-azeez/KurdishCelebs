using KurdishCelebs.WebApp.Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.IO;
using System.Linq;

namespace KurdishCelebs.WebApp.Helpers
{
    public static class ImageProcessingHelper
    {
        public static string ConvertToBase64(this Stream stream)
        {
            var bytes = new Byte[(int)stream.Length];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, (int)stream.Length);

            return Convert.ToBase64String(bytes);
        }

        public static MemoryStream CreateImage(string originalImagePath, SearchResult result)
        {
            MemoryStream memoryStream = new MemoryStream();

            const int imageWidth = 500;

            var contentFont = SystemFonts.CreateFont("Arial", 28, FontStyle.Regular);
            var watermarkFont = SystemFonts.CreateFont("Arial", 26, FontStyle.Bold);

            var textOptions = new TextGraphicsOptions
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var top = result.Matches.First();

            using (var original = Image.Load(originalImagePath))
            using (var match = Image.Load(top.ImagePath))
            using (var resultImage = new Image<Rgba32>(imageWidth * 2, imageWidth))
            {
                const float rectWidth = 120;
                const float rectHeight = 75;
                var rectangle = new RectangleF(imageWidth - rectWidth, (imageWidth / 2) - rectHeight, rectWidth * 2, rectHeight * 2);
                var center = new PointF(rectangle.Left + (rectangle.Width / 2), rectangle.Top + (rectangle.Height / 2));

                var nameCenter = new PointF(center.X, center.Y - (rectangle.Height / 4));
                var confidenceCenter = new PointF(center.X, center.Y + (rectangle.Height / 4));
                var watermarkCenter = new PointF(140, imageWidth - 50);
                var waterMark = "celebs.mazeez.dev";

                var faceRectangle = new Rectangle(
                            result.FaceLocation.Left,
                            result.FaceLocation.Top,
                            result.FaceLocation.Right - result.FaceLocation.Left,
                            result.FaceLocation.Bottom - result.FaceLocation.Top);

                var thickNess = (original.Width * original.Height) > (1000 * 1000) ? 10 :
                                (original.Width * original.Height) > (500 * 500) ? 5 : 2;

                original.Mutate(x => x.Draw(Color.Blue, thickNess, faceRectangle).Resize(imageWidth, imageWidth, true));
                match.Mutate(x => x.Resize(imageWidth, imageWidth, true));

                resultImage.Mutate(o => o
                  .DrawImage(original, new Point(0, 0), 1f)
                  .DrawImage(match, new Point(imageWidth, 0), 1f)
                  .Fill(Color.Red, rectangle)
                  .DrawText(textOptions, top.Name, contentFont, Color.White, nameCenter)
                  .DrawText(textOptions, $"{top.Confidence:p1}", contentFont, Color.White, confidenceCenter)
                  .DrawText(textOptions, waterMark, watermarkFont, Brushes.Solid(Rgba32.White), Pens.Solid(Rgba32.Black, 1), watermarkCenter)
                );

                resultImage.Save(memoryStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
            }

            return memoryStream;
        }

    }
}
