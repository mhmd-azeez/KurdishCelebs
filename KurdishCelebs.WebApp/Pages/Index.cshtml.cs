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
    public class UploadRequest
    {
        public string Image { get; set; }
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

        public async Task<IActionResult> OnPost([FromBody] UploadRequest request)
        {
            try
            {
                var filePath = Path.GetTempFileName();

                var image = request.Image.Substring("data:image/jpeg;base64,".Length);

                using (var memStream = new MemoryStream(Convert.FromBase64String(image)))
                using (var stream = System.IO.File.Create(filePath))
                {
                    await memStream.CopyToAsync(stream);
                }

                var result = _recognitionService.Search(filePath);

                var top = result.Matches.FirstOrDefault();

                using var memoryStream = ImageProcessingHelper.CreateImage(filePath, result);

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
    }
}
