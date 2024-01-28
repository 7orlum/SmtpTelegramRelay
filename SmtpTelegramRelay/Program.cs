using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using System.Runtime.InteropServices;

namespace SmtpTelegramRelay;

public static class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("appsettings.yaml", optional: true)
            .AddYamlFile($"appsettings.{builder.Environment.EnvironmentName}.yaml", optional: true);
        builder.Services
            .AddHostedService<Relay>()
            .Configure<RelayConfiguration>(builder.Configuration);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            builder.Services.AddWindowsService(options => options.ServiceName = "SMTP Telegram Relay");
            LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);
        }
        else
        {
            builder.Services.AddSystemd();
        }

        var host = builder.Build();
        host.Run();
    }
}