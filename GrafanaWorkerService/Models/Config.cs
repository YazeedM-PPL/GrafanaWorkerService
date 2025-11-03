using CsToml;

namespace GrafanaWorkerService.Models
{
    public partial class Config
    {

        public List<GroupChatConfig> Groups { get; set; } = new();

    }
}
