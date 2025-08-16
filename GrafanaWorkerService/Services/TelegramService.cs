using GrafanaWorkerService.Interfaces;
using GrafanaWorkerService.Models;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace GrafanaWorkerService.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly IGrafanaImageService _grafanaImageService;
        private TelegramBotClient _bot;
        private string _chatId;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TelegramService> _logger;



        public TelegramService(IGrafanaImageService grafanaImageService, IConfiguration configuration)
        {
            _grafanaImageService = grafanaImageService;
            _configuration = configuration;
        }

        public async Task SendPanelImageAlert(string panelUrl, string alertMessage)
        {
            string jsonSecretsPath = _configuration["TelegramConfig:jsonSecrets"];
            string json = File.ReadAllText(jsonSecretsPath);
            JObject config = JObject.Parse(json);
            string token = config["ApiToken"]?.ToString();
            _bot = new TelegramBotClient(token);

            _chatId = _configuration["TelegramConfig:chatId"];
            long _chatIdLong = long.Parse(_chatId);

            string imageFilePath = await _grafanaImageService.GenerateImageFile(panelUrl);

            try
            {
                using var stream = File.OpenRead(imageFilePath);

                var result = await _bot.SendPhoto(
                    chatId: _chatId,
                    photo: stream,
                    caption: alertMessage
                );
            }
            finally
            {
                if (File.Exists(imageFilePath))
                {
                    try
                    {
                        File.Delete(imageFilePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete image file: {ex.Message}");
                    }
                }
            }
        }

        public async Task TestAlerts()
        {
            string filePath = _configuration["TelegramConfig:hourlyAlertStore"];
            string _apiKey = _configuration["TelegramConfig:botToken"];

            _logger.LogInformation($" LOG FILE PATH: {filePath}");
            _logger.LogInformation($" API KEY: {_apiKey}");

            _logger.LogInformation("Base Directory: {dir}", AppContext.BaseDirectory);
            _logger.LogInformation("Current Directory: {dir}", Directory.GetCurrentDirectory());


            string json = File.ReadAllText(filePath);
            List<TelegramHourlyAlert> hourlyAlerts = JsonConvert.DeserializeObject<List<TelegramHourlyAlert>>(json);

            _logger.LogInformation("Sending test alerts.....");

            foreach (var alert in hourlyAlerts)
            {
                await SendPanelImageAlert(alert.panelUrl, alert.alertMessage);
                Thread.Sleep(60000); // 60000 milliseconds = 1 minute
                _logger.LogInformation($"Alert sent: {alert.alertMessage}");
            }
        }

        public async Task SendTestMessage()
        {
            string jsonSecretsPath = _configuration["TelegramConfig:pathToSecrets"];

            string json = File.ReadAllText(jsonSecretsPath);
            JObject config = JObject.Parse(json);
            string token = config["ApiToken"]?.ToString();
            _bot = new TelegramBotClient(token);
            _chatId = _configuration["TelegramConfig:chatId"];
            await _bot.SendMessage(_chatId, "test");
        }

    }
}
