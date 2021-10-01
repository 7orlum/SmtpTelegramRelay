using NLog;
using SmtpServer;
using SmtpServer.Net;
using SmtpServer.Tracing;
using System;
using System.IO;

namespace SmtpTelegramRelay
{
    class SmtpSessionLogger
    {
        Logger _log;

        public SmtpSessionLogger(SmtpServer.SmtpServer server, Logger logger)
        {
            server.SessionCreated += OnSessionCreated;
            server.SessionCompleted += OnSessionCompleted;
            server.SessionFaulted += OnSessionFaulted;
            server.SessionCancelled += OnSessionCancelled;

            _log = logger;
        }

        void OnSessionFaulted(object sender, SessionFaultedEventArgs e)
        {
            _log.Trace(e.Exception, "Session faulted");
        }

        void OnSessionCancelled(object sender, SessionEventArgs e)
        {
            _log.Trace("Session cancelled");
        }

        void OnSessionCreated(object sender, SessionEventArgs e)
        {
            _log.Trace($"Session created: {e.Context.Properties[EndpointListener.RemoteEndPointKey]}");

            e.Context.CommandExecuting += OnCommandExecuting;
            e.Context.CommandExecuted += OnCommandExecuted;
        }

        void OnCommandExecuting(object sender, SmtpCommandEventArgs e)
        {
            _log.Trace("Command executing");

            var writer = new StringWriter();
            new TracingSmtpCommandVisitor(writer).Visit(e.Command);
            _log.Trace(writer.ToString());
        }

        void OnCommandExecuted(object sender, SmtpCommandEventArgs e)
        {
            _log.Trace("Command executed");

            var writer = new StringWriter();
            new TracingSmtpCommandVisitor(writer).Visit(e.Command);
            _log.Trace(writer.ToString());
        }

        void OnSessionCompleted(object sender, SessionEventArgs e)
        {
            _log.Trace($"Session completed: {e.Context.Properties[EndpointListener.RemoteEndPointKey]}");

            e.Context.CommandExecuting -= OnCommandExecuting;
            e.Context.CommandExecuted -= OnCommandExecuted;
        }
    }
}
