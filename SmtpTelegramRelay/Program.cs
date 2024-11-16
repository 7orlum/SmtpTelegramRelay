using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using System.Runtime.InteropServices;

namespace SmtpTelegramRelay;

internal sealed class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        _ = builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("appsettings.yaml", optional: true)
            .AddYamlFile($"appsettings.{builder.Environment.EnvironmentName}.yaml", optional: true);

        _ = builder.Services
            .AddHostedService<Relay>()
            .Configure<RelayConfiguration>(builder.Configuration)
            .AddSystemd()
            .AddWindowsService(options => options.ServiceName = "SMTP Telegram Relay");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);
        }

        var host = builder.Build();
        host.Run();
    }
}