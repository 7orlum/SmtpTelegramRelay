using Microsoft.Extensions.Options;
using SmtpServer;
using SmtpServer.Net;
using SmtpServer.Tracing;

namespace SmtpTelegramRelay;

internal sealed class Relay(ILogger<Relay> logger, IOptionsMonitor<RelayConfiguration> options) : BackgroundService
{
    private SmtpServer.SmtpServer? _server;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var serverOptions = new SmtpServerOptionsBuilder()
                .Port(options.CurrentValue.SmtpPort)
                .Build();

            var telegramStore = new SmtpServer.ComponentModel.ServiceProvider();
            telegramStore.Add(new Store(options));

            _server = new SmtpServer.SmtpServer(serverOptions, telegramStore);
            _server.SessionCreated += OnSessionCreated;
            _server.SessionCompleted += OnSessionCompleted;
            _server.SessionFaulted += OnSessionFaulted;
            _server.SessionCancelled += OnSessionCancelled;

            var result = _server.StartAsync(stoppingToken);

            return result;
        }
        catch (OperationCanceledException)
        {
            // When the stopping token is canceled, for example, a call made from services.msc,
            // we shouldn't exit with a non-zero exit code. In other words, this is expected...
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Message}", ex.Message);

            // Terminates this process and returns an exit code to the operating system.
            // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            // performs one of two scenarios:
            // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            // 2. When set to "StopHost": will cleanly stop the host, and log errors.
            //
            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code.
            Environment.Exit(1);
        }

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested && _server is not null)
        {
            _server.Shutdown();
            await _server.ShutdownTask.ConfigureAwait(false);
        }

        await base.StopAsync(stoppingToken).ConfigureAwait(false);
    }

    void OnSessionCreated(object? sender, SessionEventArgs e)
    {
        logger.LogDebug("{Session} session created",
            e.Context.Properties[EndpointListener.RemoteEndPointKey]);

        e.Context.CommandExecuting += OnCommandExecuting;
    }

    void OnSessionCompleted(object? sender, SessionEventArgs e)
    {
        logger.LogDebug("{Session} session completed",
            e.Context.Properties[EndpointListener.RemoteEndPointKey]);

        e.Context.CommandExecuting -= OnCommandExecuting;
    }

    void OnSessionFaulted(object? sender, SessionFaultedEventArgs e)
    {
        logger.LogDebug(e.Exception, "{Session} session faulted",
            e.Context.Properties[EndpointListener.RemoteEndPointKey]);

        e.Context.CommandExecuting -= OnCommandExecuting;
    }

    void OnSessionCancelled(object? sender, SessionEventArgs e)
    {
        logger.LogDebug("{Session} session cancelled",
            e.Context.Properties[EndpointListener.RemoteEndPointKey]);

        e.Context.CommandExecuting -= OnCommandExecuting;
    }

    void OnCommandExecuting(object? sender, SmtpCommandEventArgs e)
    {
        var writer = new StringWriter();
        new TracingSmtpCommandVisitor(writer).Visit(e.Command);
        logger.LogDebug("{Session} command {Command}",
            e.Context.Properties[EndpointListener.RemoteEndPointKey],
            writer.ToString());
    }
}
