using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KurdishCelebs.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using KurdishCelebs.WebApp.Helpers;
using KurdishCelebs.WebApp.Services;

namespace KurdishCelebs.WebApp.Pages
{
    public static class StreamExtensions
    {
        public static string ConvertToBase64(this Stream stream)
        {
            var bytes = new Byte[(int)stream.Length];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, (int)stream.Length);

            return Convert.ToBase64String(bytes);
        }
    }

    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly FacialRecognitionService _recognitionService;

        public IndexModel(ILogger<IndexModel> logger, FacialRecognitionService recognitionService)
        {
            _logger = logger;
            _recognitionService = recognitionService;
        }

        public void OnGet()
        {

        }


        public async Task<IActionResult> OnPost(IFormFile file)
        {
            try
            {
                var filePath = Path.GetTempFileName();

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                var result = _recognitionService.Search(filePath);

                foreach (var match in result.Matches)
                {
                    match.Confidence = Curve(match.Confidence);
                }

                var top = result.Matches.FirstOrDefault();

                using var memoryStream = new MemoryStream();

                const int imageWidth = 500;

                var contentFont = SystemFonts.CreateFont("Arial", 36, FontStyle.Regular);
                var watermarkFont = SystemFonts.CreateFont("Arial", 26, FontStyle.Bold);

                var textOptions = new TextGraphicsOptions
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                using (var original = Image.Load(filePath))
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

                var base64 = $"data:image/jpeg;base64,{memoryStream.ConvertToBase64()}";

                System.IO.File.Delete(filePath);

                return new OkObjectResult(new
                {
                    name = top.Name,
                    confidence = top.Confidence,
                    image = base64,
                });
            }
            catch (NoFaceFoundException)
            {
                return BadRequest(new
                {
                    message = "This image doesn't contain any faces."
                });
            }
        }

        private static double Curve(double confidence)
        {
            return Math.Sqrt(confidence);
        }
    }
}
