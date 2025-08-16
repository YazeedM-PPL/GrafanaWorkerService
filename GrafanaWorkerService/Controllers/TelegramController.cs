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

        public TelegramController(ITelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        [HttpGet("SendTestAlerts")]
        public IActionResult Test()
        {
            _telegramService.TestAlerts();
            return Ok("Hello from MyController!");
        }

        [HttpGet("SendTestMessage")]
        public IActionResult SendTestMessage()
        {
            _telegramService.SendTestMessage();
            return Ok("Test message sent");
        }

    }
}
