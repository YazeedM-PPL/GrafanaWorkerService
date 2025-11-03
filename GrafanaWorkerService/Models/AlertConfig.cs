using CsToml;

namespace GrafanaWorkerService.Models
{
    public partial class AlertConfig
    {
        public string Message { get; set; } = "";
        public string Url { get; set; } = "";
    }
}
