using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrafanaWorkerService.Interfaces
{
    public interface ITelegramService
    {

        Task SendPanelImageAlert(string panelUrl, string alertMessage);

        Task TestAlerts();

        Task SendTestMessage();

    }
}
