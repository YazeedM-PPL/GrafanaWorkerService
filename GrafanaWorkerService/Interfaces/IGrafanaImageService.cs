using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrafanaWorkerService.Interfaces
{
    public interface IGrafanaImageService
    {

        Task<string> GenerateImageFile(string panelUrl);

    }
}
