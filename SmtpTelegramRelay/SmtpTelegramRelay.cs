using System;
using System.ServiceProcess;
using System.Threading;
using NLog;


namespace SmtpTelegramRelay
{
    class SmtpTelegramRelay: ServiceBase
    {
        public SmtpTelegramRelay()
        {
            this.ServiceName = Resources.ApplicationName;
        }


        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var telegramSettings = TelegramConfiguration.Read();
            var telegram = new TelegramAsMessageStore(telegramSettings.Token, telegramSettings.ChatId, telegramSettings.Proxy);

            var smtpSettings = SmtpConfiguration.Read();
            var options = new SmtpServer.SmtpServerOptionsBuilder()
                .Port(smtpSettings.Port)
                .MessageStore(telegram)
                .Build();
            var smtpServer = new SmtpServer.SmtpServer(options);
            _ = smtpServer.StartAsync(_cancellationTokenSource.Token);

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
