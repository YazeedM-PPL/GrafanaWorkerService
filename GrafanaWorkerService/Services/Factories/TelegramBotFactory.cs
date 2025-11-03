using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrafanaWorkerService.Services.Factories
{
    public static class TelegramBotFactory
    {

        public static IServiceCollection AddTelegramBots(this IServiceCollection services, IConfiguration configuration, Serilog.ILogger logger)
        {
            string? secretsPath = configuration["TelegramConfig:pathToSecrets"];

            if (string.IsNullOrWhiteSpace(secretsPath))
            {
                logger.Error("TelegramConfig:pathToSecrets is missing from configuration.");
                throw new InvalidOperationException("TelegramConfig:pathToSecrets is missing.");
            }

            string json = File.ReadAllText(secretsPath);
            JObject config = JObject.Parse(json);

            string? token = config["ApiToken"]?.ToString();

            if (string.IsNullOrWhiteSpace(token))
            {
                logger.Error("Telegram bot token is missing from the secrets file.");
                throw new InvalidOperationException("Telegram bot token is missing.");
            }

            var defaultBot = new Telegram.Bot.TelegramBotClient(token);

            services.AddSingleton(defaultBot);

            services.AddSingleton<Interfaces.ITelegramService, Services.TelegramService>();

            logger.Information("Telegram bot client registered successfully from {Path}", secretsPath);

            return services;

        }

    }
}
