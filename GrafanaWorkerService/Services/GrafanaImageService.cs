using GrafanaWorkerService.Interfaces;
using Newtonsoft.Json.Linq;
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
        private readonly IConfiguration _configuration;

        public GrafanaImageService(HttpClient httpClient, ILogger<GrafanaImageService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        private static string ConvertToRenderUrl(string panelUrl)
        {
            if (string.IsNullOrEmpty(panelUrl))
            {
                throw new ArgumentException("panelUrl cannot be empty");
            }

            var uri = new Uri(panelUrl);
            var query = HttpUtility.ParseQueryString(uri.Query);

            var orgId = query.Get("orgId") ?? "1";
            var from = query.Get("from") ?? "now-6h";
            var to = query.Get("to") ?? "now";
            var viewPanel = query.Get("viewPanel");

            if (string.IsNullOrEmpty(viewPanel))
                throw new ArgumentException("No viewPanel parameter found in the input URL");

            // split the path
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 2)
                throw new ArgumentException("Invalid Grafana panel URL structure");

            // /d/<uid>/<slug> or /d/<uid>
            string dashboardUid = segments[1];
            string slug = segments.Length >= 3 ? segments[2] : "fakeSlug";

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

            string jsonSecretsPath = _configuration["TelegramConfig:pathToSecrets"];
            string json = File.ReadAllText(jsonSecretsPath);
            JObject config = JObject.Parse(json);
            string bearerToken = config["GrafanaToken"]?.ToString();

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, renderUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(tempFile, imageBytes);

                return tempFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate image file from Grafana panel URL");
                throw;
            }
        }

    }
}
