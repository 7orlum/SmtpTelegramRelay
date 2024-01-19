using NLog;
using SmtpServer;
using SmtpServer.ComponentModel;
using System;
using System.Threading;
using Topshelf;

namespace SmtpTelegramRelay
{
    class Relay: ServiceControl, IDisposable
    {
        CancellationTokenSource _cancellationTokenSource;
        NLog.Logger _log = LogManager.GetCurrentClassLogger();
        bool _disposed;

        public bool Start(HostControl hostControl)
        {
            var smtpSettings = SmtpConfiguration.Read();
            var telegramSettings = TelegramConfiguration.Read();

            var options = new SmtpServerOptionsBuilder()
                .Port(smtpSettings.Port)
                .Build();

            var provider = new ServiceProvider();
            provider.Add(new Store(telegramSettings.Token, telegramSettings.ChatId));

            var smtpServer = new SmtpServer.SmtpServer(options, provider);
            _ = new Logger(smtpServer, _log);

            _cancellationTokenSource = new();
            smtpServer.StartAsync(_cancellationTokenSource.Token).Wait();

            if (Environment.UserInteractive)
                _log.Warn($"{Resources.ApplicationName} started in interactive mode");
            else
                _log.Warn($"{Resources.ApplicationName} service started");

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            if (_cancellationTokenSource != null)
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource.Cancel();

                _cancellationTokenSource.Dispose();
            }

            if (Environment.UserInteractive)
                _log.Warn($"{Resources.ApplicationName} program stopped");
            else
                _log.Warn($"{Resources.ApplicationName} service stopped");

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Dispose();

            _disposed = true;
        }
    }
}
