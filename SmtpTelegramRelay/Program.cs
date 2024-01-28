namespace SmtpTelegramRelay;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("appsettings.yaml", optional: true)
            .AddYamlFile($"appsettings.{builder.Environment.EnvironmentName}.yaml", optional: true);
        builder.Services
            .AddHostedService<Relay>()
            .Configure<RelayConfiguration>(builder.Configuration);

        var host = builder.Build();
        host.Run();
    }
}