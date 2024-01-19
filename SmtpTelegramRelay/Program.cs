namespace SmtpTelegramRelay;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Relay>();
        
        var host = builder.Build();
        host.Run();
    }
}