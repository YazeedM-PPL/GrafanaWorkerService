using GrafanaWorkerService.Interfaces;
using GrafanaWorkerService.Services;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace GrafanaWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var builder = Host.CreateApplicationBuilder(args);
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddWindowsService();
            //builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("C:\\GrafanaWorkerService\\Logs\\Logs.log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddHttpClient<IGrafanaImageService, GrafanaImageService>();

            builder.Services.AddSingleton<ITelegramService, TelegramService>();


            var host = builder.Build();
            host.UseSwagger();
            host.UseSwaggerUI();
            host.MapControllers();
            host.Run();
        }
    }
}


// sc.exe create "GrafanaWorkerService" binPath= "C:\Users\yazeedm\YAZEED\MyProjects\GrafanaWorkerService\Precompiled\GrafanaWorkerService.exe" DisplayName= "Grafana Worker Service" start= auto
