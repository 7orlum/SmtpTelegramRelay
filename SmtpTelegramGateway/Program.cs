using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using SmtpServer.Storage;
using System.Runtime.InteropServices;

namespace SmtpTelegramGateway;

internal sealed class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        _ = builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("appsettings.yaml", optional: true, reloadOnChange: true)
            .AddYamlFile($"appsettings.{builder.Environment.EnvironmentName}.yaml", optional: true, reloadOnChange: true);

        _ = builder.Services
            .AddHostedService<Smtp>()
            .AddSingleton<MessageStore, Telegram>()
            .Configure<Configuration>(builder.Configuration)
            .AddSystemd()
            .AddWindowsService(options => options.ServiceName = "SMTP Telegram Gateway");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);
        }

        var host = builder.Build();
        host.Run();
    }
}