using GrafanaWorkerService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GrafanaWorkerService.Services
{
    public class GrafanaImageService : IGrafanaImageService
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<GrafanaImageService> _logger;

        public GrafanaImageService(HttpClient httpClient, ILogger<GrafanaImageService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private string ConvertToRenderUrl(string panelUrl)
        {
            if (string.IsNullOrEmpty(panelUrl))
            {
                throw new ArgumentException("panelUrl cannot be empty");

            }

            var uri = new Uri(panelUrl);
            var query = HttpUtility.ParseQueryString(uri.Query);

            var orgId = query.Get("ordId") ?? "1";
            var from = query.Get("from") ?? "now-6h";
            var to = query.Get("to") ?? "now";
            var viewPanel = query.Get("viewPanel");

            if (string.IsNullOrEmpty(viewPanel))
                throw new ArgumentException("No viewPanel parameter found in the input URL");

            // split the path
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 3)
                throw new ArgumentException("Invalid Grafana panel URL structure");

            // /d/<uid>/<slug>
            string dashboardUid = segments[1];
            string slug = segments[2];

            // build the render URL
            var builder = new UriBuilder(uri.Scheme, uri.Host, uri.Port);
            builder.Path = $"/render/d-solo/{dashboardUid}/{slug}";
            builder.Query = $"orgId={orgId}&from={from}&to={to}&panelId={viewPanel}";

            return builder.ToString();
        }

        public async Task<string> GenerateImageFile(string panelUrl)
        {
            string renderUrl = ConvertToRenderUrl(panelUrl);
            var tempFile = Path.Combine(Path.GetTempPath(), $"GRAFANA_panel_{Guid.NewGuid()}.png");
            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(this.ConvertToRenderUrl(panelUrl));
                await File.WriteAllBytesAsync(tempFile, imageBytes);
                return tempFile;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
