using CsToml;

namespace GrafanaWorkerService.Models
{
    public partial class GroupChatConfig
    {
        public string Name { get; set; } = "";

        public long Id { get; set; }

        public int StartHour { get; set; }

        public int EndHour { get; set; }

        public List<AlertConfig> Alerts { get; set; } = new();

    }
}
