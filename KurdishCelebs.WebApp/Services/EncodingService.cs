using KurdishCelebs.Shared;
using KurdishCelebs.WebApp.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KurdishCelebs.WebApp.Services
{
    public class EncodingService : IHostedService
    {
        private readonly FacialRecognitionService _recognitionService;
        private readonly ILogger<EncodingService> _logger;

        public EncodingService(FacialRecognitionService recognitionService, ILogger<EncodingService> logger)
        {
            _recognitionService = recognitionService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var models1 = Environment.GetEnvironmentVariable("KURDCELEBS_MODELS_DIR", EnvironmentVariableTarget.Machine);
            var models2 = Environment.GetEnvironmentVariable("KURDCELEBS_MODELS_DIR");

            if ((models1 ?? models2) is null)
            {
                throw new InvalidOperationException("Please set KURDCELEBS_MODELS_DIR environment variable.");
            }

            _logger.LogWarning($"models1: {models1}");
            _logger.LogWarning($"models2: {models2}");

            _recognitionService.Initialize();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
