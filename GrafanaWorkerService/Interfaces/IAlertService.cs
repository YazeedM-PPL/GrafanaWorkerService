using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrafanaWorkerService.Interfaces
{
    public interface IAlertService
    {

        Task TriggerHourlyAlerts(CancellationToken cancellationToken, bool checkAlertWindow = true);

        Task HandleIncomingAlert(JsonElement payload);

    }
}
