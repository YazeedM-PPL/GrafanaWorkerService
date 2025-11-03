using GrafanaWorkerService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrafanaWorkerService.Interfaces
{
    public interface ITelegramService
    {

        Task SendHourlyAlert(long groupChatId, AlertConfig alert);

        bool IsWithinAlertWindow(GroupChatConfig group);

        Task SendImageMessage(long chatId, string panelUrl, string message);

        Task SendMessage(long chatId, string message);

    }
}
