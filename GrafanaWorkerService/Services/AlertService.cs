using GrafanaWorkerService.Interfaces;
using GrafanaWorkerService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GrafanaWorkerService.Services
{
    public class AlertService : IAlertService
    {

        private readonly ITelegramService _telegramService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AlertService> _logger;

        public AlertService(
            ITelegramService telegramService,
            IConfiguration configuration,
            ILogger<AlertService> logger
        )
        {
            _telegramService = telegramService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task TriggerHourlyAlerts(CancellationToken cancellationToken, bool checkAlertWindow = true)
        {
            // Load Config file
            var yamlPath = _configuration["TelegramConfig:configFile"];

            if (!File.Exists(yamlPath))
            {
                _logger.LogWarning("Config file not found at path: {Path}", yamlPath);
                return;
            }

            var yaml = File.ReadAllText(yamlPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(NullNamingConvention.Instance)
                .Build();

            Config config = deserializer.Deserialize<Config>(yaml);

            foreach (GroupChatConfig groupChat in config.Groups)
            {
                if (groupChat.Alerts != null && groupChat.Alerts.Any())
                {
                    if (!checkAlertWindow || _telegramService.IsWithinAlertWindow(groupChat))
                    {
                        foreach (AlertConfig alert in groupChat.Alerts)
                        {
                            await _telegramService.SendHourlyAlert(groupChat.Id, alert);
                            await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Skipping alerts for group {GroupName} due to alert window", groupChat.Name);
                    }
                }
                else
                {
                    _logger.LogInformation("No alerts for group {GroupName}", groupChat.Name);
                }
            }
        }


        public async Task HandleIncomingAlert(JsonElement payload)
        {

            long _chatId = long.Parse(_configuration["TelegramConfig:groupChats:CRITICAL_ALERTS"]);

            using JsonDocument doc = JsonDocument.Parse(payload.GetRawText());

            string? panelUrl = doc.RootElement
                .GetProperty("alerts")[0]
                .GetProperty("panelURL")
                .GetString();

            string? status = doc.RootElement
                .GetProperty("status")
                .GetString();

            string? summary = doc.RootElement
                .GetProperty("alerts")[0]
                .GetProperty("annotations")
                .GetProperty("summary")
                .GetString();

            string alertCaption = status switch
            {
                "firing" => $"""
            🔴 <b>Firing</b>

            <b>Summary:</b>
            {summary}
            """,
                "resolved" => $"""
            🟢 <b>Resolved</b>

            <b>Summary:</b>
            {summary}
            """,
                _ => $"""
            ⚪ <b>Unknown Status</b>

            <b>Summary:</b>
            {summary}
            """
            };

            if (panelUrl == null || panelUrl! == "")
            {
                await _telegramService.SendMessage(_chatId, alertCaption);
            }
            else
            {
                await _telegramService.SendImageMessage(_chatId, panelUrl, alertCaption);
            }

        }

    }
}
