using GrafanaWorkerService.Interfaces;
using GrafanaWorkerService.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GrafanaWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITelegramService _telegramSevice;
        private readonly IAlertService _alertService;

        public Worker(
            ILogger<Worker> logger,
            IConfiguration configuration,
            ITelegramService telegramService,
            IAlertService alertService)
        {
            _logger = logger;
            _configuration = configuration;
            _telegramSevice = telegramService;
            _alertService = alertService;
        }


        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {

            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);


            while (!cancellationToken.IsCancellationRequested)
            {

                try
                {
                    var now = DateTime.Now;
                    var nextHour = now.AddHours(1).Date.AddHours(now.AddHours(1).Hour);
                    var delay = nextHour - now;

                    _logger.LogInformation($"Next alerting hour: {nextHour}");

                    await Task.Delay(delay, cancellationToken);

                    await _alertService.TriggerHourlyAlerts(cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in worker loop");
                }

            }
        }
    }
}
