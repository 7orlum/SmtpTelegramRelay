using Microsoft.Extensions.Options;
using SmtpServer;
using SmtpServer.Storage;

namespace SmtpTelegramRelay;

internal sealed class Relay(MessageStore store, ILogger<Relay> logger, IOptionsMonitor<RelayConfiguration> options) : BackgroundService
{
    private SmtpServer.SmtpServer? _server;

#pragma warning disable CA1031 // Do not catch general exception types
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var serverOptions = new SmtpServerOptionsBuilder()
                .Port(options.CurrentValue.SmtpPort)
                .Build();

            var serviceProvider = new SmtpServer.ComponentModel.ServiceProvider();
            serviceProvider.Add(store);

            _server = new SmtpServer.SmtpServer(serverOptions, serviceProvider);
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
            logger.Error(ex);

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
#pragma warning restore CA1031 // Do not catch general exception types

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested && _server is not null)
        {
            _server.Shutdown();
            await _server.ShutdownTask.ConfigureAwait(false);
        }

        await base.StopAsync(stoppingToken).ConfigureAwait(false);
    }

    private void OnSessionCreated(object? sender, SessionEventArgs e)
    {
        logger.DebugSessionCreated(e);
        e.Context.CommandExecuting += OnCommandExecuting;
    }

    private void OnSessionCompleted(object? sender, SessionEventArgs e)
    {
        logger.DebugSessionCompleted(e);
        e.Context.CommandExecuting -= OnCommandExecuting;
    }

    private void OnSessionCancelled(object? sender, SessionEventArgs e)
    {
        logger.DebugSessionCancelled(e);
        e.Context.CommandExecuting -= OnCommandExecuting;
    }

    private void OnSessionFaulted(object? sender, SessionFaultedEventArgs e)
    {
        logger.DebugSessionFaulted(e);
        e.Context.CommandExecuting -= OnCommandExecuting;
    }

    private void OnCommandExecuting(object? sender, SmtpCommandEventArgs e)
    {
        logger.DebugCommandExecuting(e);
    }
}
