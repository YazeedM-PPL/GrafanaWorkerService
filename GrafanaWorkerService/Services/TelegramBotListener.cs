using GrafanaWorkerService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GrafanaWorkerService.Services
{



    public class TelegramBotListener : BackgroundService
    {
        private readonly TelegramBotClient _bot;
        private readonly ILogger<TelegramBotListener> _logger;
        private readonly IAlertService _alertService;
        private readonly ITelegramService _telegramService;

        public TelegramBotListener(
            TelegramBotClient bot,
            IAlertService alertService,
            ILogger<TelegramBotListener> logger,
            ITelegramService telegramService)
        {
            _bot = bot;
            _alertService = alertService;
            _logger = logger;
            _telegramService = telegramService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var me = await _bot.GetMe(stoppingToken);
            _logger.LogInformation("Telegram bot @{Username} started listening...", me.Username);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            _bot.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );

            await Task.Delay(-1, stoppingToken); // keep running
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Message is not { } message)
                return;

            if (message.Text is null)
                return;

            _logger.LogInformation("Received message '{Text}' from {User}", message.Text, message.From?.Username);

            // Simple example: echo the received message
            //await bot.SendMessage(
            //    chatId: message.Chat.Id,
            //    text: $"{message.From?.Username} said: {message.Text}",
            //    parseMode: ParseMode.Html,
            //    cancellationToken: token
            //);

            if (message.Text == "/update")
            {
                await _bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Grafana Updates Incoming...",
                    cancellationToken: token
                );
                await _alertService.TriggerHourlyAlerts(new System.Threading.CancellationToken(), false);
            }

        }

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            _logger.LogError(exception, "Error occurred in Telegram bot polling");
            return Task.CompletedTask;
        }

    }
}
