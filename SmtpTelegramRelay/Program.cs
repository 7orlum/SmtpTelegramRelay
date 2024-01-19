using System;
using System.ServiceProcess;
using System.Threading;
using System.Reflection;
using System.Diagnostics.Contracts;
using System.Security;
using System.Runtime.ExceptionServices;
using System.Globalization;
using NLog;
using Topshelf;


namespace SmtpTelegramRelay
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //gracefully close logger when the process terminates
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => { LogManager.Flush(); LogManager.Shutdown(); Thread.Sleep(1000); };
            AppDomain.CurrentDomain.DomainUnload += (object sender, EventArgs e) => { LogManager.Flush(); LogManager.Shutdown(); Thread.Sleep(1000); };

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(LogUnhandledException);

            if (args != null && args.Length > 0)
                Install(args);
            else if (Environment.UserInteractive)
                RunInteractively();
            else
                RunAsService();

            LogManager.Flush();
            LogManager.Shutdown();

            //take NLog.Telegram a chanсe to send an error message
            Thread.Sleep(1000);
        }


        static void Configure()
        {
            HostFactory.Run(x =>
            {
                x.Service<Relay>(service =>
                {
                    service.ConstructUsing(s => new MyService());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                x.RunAsLocalSystem();
                x.SetServiceName("MyWindowServiceWithTopshelf");
            });
        }

        static void RunAsService()
        {
            try
            {
                ServiceBase.Run(new ServiceBase[] { new Relay() });
            }
            catch (Exception e)
            {
                _log.Fatal(e);
            }
        }


        static void RunInteractively()
        {
            Console.TreatControlCAsInput = true;

            Console.WriteLine(Resources.Help);
            Console.WriteLine();
            Console.WriteLine($"{Resources.ApplicationName} is runnig. Press any key to stop...");
            Console.WriteLine();

            var relay = new Relay();
            relay.Start();

            Console.ReadKey(true);

            relay.Stop();
        }


        static void Install(string[] args)
        {
            Contract.Requires(args != null);

            if (args.Length == 1 && args[0] != null)
            {
                switch (args[0].ToLower(CultureInfo.InvariantCulture))
                {
                    case "install":
                        try
                        {
                            ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                        }
                        catch (Exception e)
                        {
                            _log.Error(e, "Failed to install service.");
                        }
                        break;
                    case "uninstall":
                        try
                        {
                            ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                        }
                        catch (Exception e)
                        {
                            _log.Error(e, "Failed to uninstall service.");
                        }
                        break;
                    default:
                        Console.WriteLine(Resources.Help);
                        break;
                }
            }
            else
                Console.WriteLine(Resources.Help);
        }


        [SecurityCritical, HandleProcessCorruptedStateExceptions]
        static void LogUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            if (args.IsTerminating)
                _log.Fatal((Exception)args.ExceptionObject, "Unhandled exception occure, runtime is terminating");
            else
                _log.Fatal((Exception)args.ExceptionObject, "Unhandled exception occure, runtime is not terminating");

            LogManager.Flush();
            LogManager.Shutdown();

            //take NLog.Telegram a chanсe to send an error message
            Thread.Sleep(1000);
        }


        static readonly NLog.Logger _log = LogManager.GetCurrentClassLogger();
    }
}
