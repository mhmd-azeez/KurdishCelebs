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
using Microsoft.AspNetCore.Hosting;

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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexModel(ILogger<IndexModel> logger,
            FacialRecognitionService recognitionService,
            IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _recognitionService = recognitionService;
            _webHostEnvironment = webHostEnvironment;
        }

        public void OnGet()
        {

        }


        public async Task<IActionResult> OnPost(IFormFile file)
        {
            var filePath = Path.GetTempFileName();

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var matches = _recognitionService.Search(filePath);

            foreach (var match in matches)
            {
                match.Confidence = Curve(match.Confidence);
            }

            var top = matches.FirstOrDefault();

            using var memoryStream = new MemoryStream();

            const int imageWidth = 500;

            var contentFont = SystemFonts.CreateFont("Arial", 36, FontStyle.Regular);
            var watermarkFont = SystemFonts.CreateFont("Arial", 28, FontStyle.Bold);

            var textOptions = new TextGraphicsOptions
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var id = Guid.NewGuid().ToString("N");

            using (var original = Image.Load(filePath))
            using (var match = Image.Load(top.ImagePath))
            using (var result = new Image<Rgba32>(imageWidth * 2, imageWidth))
            {

                const float rectWidth = 120;
                const float rectHeight = 75;
                var rectangle = new RectangleF(imageWidth - rectWidth, (imageWidth / 2) - rectHeight, rectWidth * 2, rectHeight * 2);
                var center = new PointF(rectangle.Left + (rectangle.Width / 2), rectangle.Top + (rectangle.Height / 2));

                var nameCenter = new PointF(center.X, center.Y - (rectangle.Height / 4));
                var confidenceCenter = new PointF(center.X, center.Y + (rectangle.Height / 4));
                var watermarkCenter = new PointF(100, imageWidth - 50);
                var waterMark = "mazeez.dev";

                original.Mutate(x => x.Resize(imageWidth, imageWidth, true));
                match.Mutate(x => x.Resize(imageWidth, imageWidth, true));

                result.Mutate(o => o
                  .DrawImage(original, new Point(0, 0), 1f)
                  .DrawImage(match, new Point(imageWidth, 0), 1f)
                  .Fill(Color.Red, rectangle)
                  .DrawText(textOptions, top.Name, contentFont, Color.White, nameCenter)
                  .DrawText(textOptions, $"{top.Confidence:p1}", contentFont, Color.White, confidenceCenter)
                  .DrawText(textOptions, waterMark, watermarkFont, Brushes.Solid(Rgba32.White), Pens.Solid(Rgba32.Black, 1), watermarkCenter)
                );

                var resultFolder = Path.Combine(_webHostEnvironment.WebRootPath, PathHelper.ResultFolder);
                var resultFilePath = Path.Combine(resultFolder, $"{id}.jpg");
                if (Directory.Exists(resultFolder) == false)
                {
                    Directory.CreateDirectory(resultFolder);
                }

                result.Save(resultFilePath);
            }

            System.IO.File.Delete(filePath);

            return new OkObjectResult(new
            {
                id = id,
            });
        }

        private static double Curve(double confidence)
        {
            return Math.Sqrt(confidence);
        }
    }
}
