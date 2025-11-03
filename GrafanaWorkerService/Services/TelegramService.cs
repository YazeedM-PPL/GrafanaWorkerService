using GrafanaWorkerService.Interfaces;
using GrafanaWorkerService.Models;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.Enums;

namespace GrafanaWorkerService.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly IGrafanaImageService _grafanaImageService;
        private TelegramBotClient _bot;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TelegramService> _logger;

        public TelegramService(
            IGrafanaImageService grafanaImageService,
            TelegramBotClient bot,
            IConfiguration configuration,
            ILogger<TelegramService> logger
        )
        {
            _grafanaImageService = grafanaImageService;
            _bot = bot;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendHourlyAlert(long chatId, AlertConfig alert)
        {

            string imageFilePath = await _grafanaImageService.GenerateImageFile(alert.Url);

            try
            {
                using var stream = File.OpenRead(imageFilePath);

                var result = await _bot.SendPhoto(
                    chatId: chatId,
                    photo: stream,
                    caption: alert.Message
                );
            }
            finally
            {
                if (File.Exists(imageFilePath))
                {
                    _logger.LogInformation("File path: " + imageFilePath);
                    try
                    {
                        File.Delete(imageFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete image file");
                    }
                }
            }
        }

        public async Task SendImageMessage(long chatId, string panelUrl, string message)
        {
            string imageFilePath = await _grafanaImageService.GenerateImageFile(panelUrl);
            Console.WriteLine("\nAbout to send message: " + message);

            try
            {
                using var stream = File.OpenRead(imageFilePath);
                var result = await _bot.SendPhoto(
                    chatId: chatId,
                    photo: stream,
                    caption: message,
                    ParseMode.Html
                );
            }
            catch(Exception e)
            {
                if (File.Exists(imageFilePath))
                {
                    _logger.LogInformation("File path: " + imageFilePath);
                    try
                    {
                        File.Delete(imageFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete image file");
                    }
                }
            }

        }

        public async Task SendMessage(long chatId, string message)
        {
            try
            {
                await _bot.SendMessage(chatId, message, ParseMode.Html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message");
            }
        }

        public bool IsWithinAlertWindow(GroupChatConfig group)
        {
            int currentHour = DateTime.Now.Hour;

            // Handle same-day window
            if (group.StartHour <= group.EndHour)
            {
                return currentHour >= group.StartHour && currentHour < group.EndHour;
            }
            // Handle overnight window (e.g., 22 to 6)
            else
            {
                return currentHour >= group.StartHour || currentHour < group.EndHour;
            }

        }

    }
}
