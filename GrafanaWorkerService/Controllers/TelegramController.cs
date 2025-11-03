using GrafanaWorkerService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrafanaWorkerService.Controllers
{
    public class TelegramController : ControllerBase
    {

        private readonly ITelegramService _telegramService;
        private readonly IAlertService _alertService;

        public TelegramController(
            ITelegramService telegramService,
            IAlertService alertService)
        {
            _telegramService = telegramService;
            _alertService = alertService;
        }

        [HttpGet("SendTestMessage")]
        public async Task SendTestMessage(long chatId, string panelUrl, string message)
        {
            Console.WriteLine("Webhook contacted");
            await _telegramService.SendImageMessage(chatId, panelUrl, message);
        }

        [HttpGet("TriggerHourlyAlerts")]
        public async Task TriggerHourlyAlerts()
        {
            await _alertService.TriggerHourlyAlerts(new System.Threading.CancellationToken());
        }

    }
}
