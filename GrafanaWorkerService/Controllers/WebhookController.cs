using GrafanaWorkerService.Interfaces;
using GrafanaWorkerService.Models;
using GrafanaWorkerService.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace GrafanaWorkerService.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {

        private readonly ILogger<WebhookController> _logger;
        private readonly ITelegramService _telegramService;
        private readonly IAlertService _alertService;

        public WebhookController(
            ITelegramService telegramService,
            IAlertService alertService,
            ILogger<WebhookController> logger)
        {
            _telegramService = telegramService;
            _alertService = alertService;
            _logger = logger;
        }

        [HttpPost]
        public async void Receive([FromBody] JsonElement payload)
        {

            Console.WriteLine("Webhook contacted");
            Console.WriteLine(payload.ToString());
            _logger.LogInformation("Received webhook: " + payload.ToString());

            await _alertService.HandleIncomingAlert(payload);
        }

    }
}
