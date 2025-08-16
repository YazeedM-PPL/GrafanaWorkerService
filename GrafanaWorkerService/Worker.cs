using GrafanaWorkerService.Interfaces;
using GrafanaWorkerService.Models;
using GrafanaWorkerService.Services;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Web;
using Telegram.Bot;

namespace GrafanaWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private ITelegramService _telegramSevice;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, ITelegramService telegramService)
        {
            _logger = logger;
            _configuration = configuration;
            _telegramSevice = telegramService;
        }


        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {

            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

            // Define excluded hours: between 10 PM and 6 AM (22 to 5 inclusive)
            var excludedHours = Enumerable.Range(22, 2).Concat(Enumerable.Range(0, 6)).ToHashSet();

            await _telegramSevice.SendTestMessage();


            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try
                {

                    var now = DateTime.Now;
                    var nextHour = now.AddHours(1).Date.AddHours(now.AddHours(1).Hour);
                    var delay = nextHour - now;

                    _logger.LogInformation($"Next alerting hour: {nextHour}");

                    await Task.Delay(delay, cancellationToken);

                    // At the top of the hour
                    var currentHour = DateTime.Now.Hour;
                    if (excludedHours.Contains(currentHour))
                    {
                        _logger.LogInformation($"Current hour ({currentHour}) is excluded. Skipping alert.");
                        continue;
                    }
                    else
                    {
                        string filePath = _configuration["TelegramConfig:hourlyAlertStore"];
                        string _apiKey = _configuration["TelegramConfig:botToken"];

                        _logger.LogInformation($" LOG FILE PATH: {filePath}");
                        _logger.LogInformation($" API KEY: {_apiKey}");

                        _logger.LogInformation("Base Directory: {dir}", AppContext.BaseDirectory);
                        _logger.LogInformation("Current Directory: {dir}", Directory.GetCurrentDirectory());


                        string json = File.ReadAllText(filePath);
                        List<TelegramHourlyAlert> hourlyAlerts = JsonConvert.DeserializeObject<List<TelegramHourlyAlert>>(json);

                        _logger.LogInformation($"Processing {hourlyAlerts.Count} alerts for current hour: {currentHour}");

                        foreach (var alert in hourlyAlerts)
                        {
                            await _telegramSevice.SendPanelImageAlert(alert.panelUrl, alert.alertMessage);
                            Thread.Sleep(60000); // 60000 milliseconds = 1 minute
                            _logger.LogInformation($"Alert sent: {alert.alertMessage}");
                        }

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing alerts."); 
                    Thread.Sleep(10000); // 60000 milliseconds = 1 minute
                }

            }
        }

    }
}
