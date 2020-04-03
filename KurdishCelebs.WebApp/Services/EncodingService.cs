using KurdishCelebs.Shared;
using KurdishCelebs.WebApp.Helpers;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace KurdishCelebs.WebApp.Services
{
    public class EncodingService : IHostedService
    {
        private readonly FacialRecognitionService _recognitionService;

        public EncodingService(FacialRecognitionService recognitionService)
        {
            _recognitionService = recognitionService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _recognitionService.Initialize();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
