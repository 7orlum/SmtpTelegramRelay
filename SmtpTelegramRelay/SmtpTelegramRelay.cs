using NLog;
using SmtpServer;
using SmtpServer.ComponentModel;
using System;
using System.ServiceProcess;
using System.Threading;


namespace SmtpTelegramRelay
{
    class SmtpTelegramRelay: ServiceBase
    {
        public SmtpTelegramRelay()
        {
            ServiceName = Resources.ApplicationName;
        }


        public async void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var telegramSettings = TelegramConfiguration.Read();
            var telegramAsMessageStore = new TelegramAsMessageStore(telegramSettings.Token, telegramSettings.ChatId, telegramSettings.Proxy?.GetIWebProxy());

            var smtpSettings = SmtpConfiguration.Read();
            var options = new SmtpServerOptionsBuilder()
                .Port(smtpSettings.Port)
                .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(telegramAsMessageStore);

            var smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);
            _ = new SmtpSessionLogger(smtpServer, _log);
            await smtpServer.StartAsync(_cancellationTokenSource.Token);

            if (Environment.UserInteractive)
                _log.Warn($"{ServiceName} started in interactive mode");
            else
                _log.Warn($"{ServiceName} service started");
        }


        #region ServiceBase
        protected override void OnStart(string[] args)
        {
            Start();
        }


        protected override void OnStop()
        {
            if (_cancellationTokenSource != null)
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource.Cancel();

                _cancellationTokenSource.Dispose();
            }

            if (Environment.UserInteractive)
                _log.Warn($"{ServiceName} program stopped");
            else
                _log.Warn($"{ServiceName} service stopped");
        }


        protected override void Dispose(bool disposing)
        {
            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Dispose();

            base.Dispose(disposing);
        }
        #endregion ServiceBase

        
        bool CheckProgramSettings()
        {
            return true;
        }


        CancellationTokenSource _cancellationTokenSource;
        static readonly Logger _log = LogManager.GetCurrentClassLogger();
    }
}
