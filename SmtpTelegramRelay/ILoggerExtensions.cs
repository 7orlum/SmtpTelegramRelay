using SmtpServer;
using SmtpServer.Net;
using SmtpServer.Tracing;

namespace SmtpTelegramRelay;

internal static class ILoggerExtensions
{
    private static readonly Action<ILogger, string, Exception?> _error =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            0,
            "{Message}");

    private static readonly Action<ILogger, object, Exception?> _debugSessionCreated =
        LoggerMessage.Define<object>(
            LogLevel.Debug,
            1,
            "Session {Session} created");

    private static readonly Action<ILogger, object, Exception?> _debugSessionCompleted =
        LoggerMessage.Define<object>(
            LogLevel.Debug,
            2,
            "Session {Session} completed");

    private static readonly Action<ILogger, object, Exception?> _debugSessionCancelled =
        LoggerMessage.Define<object>(
            LogLevel.Debug,
            3,
            "Session {Session} cancelled");

    private static readonly Action<ILogger, object, Exception?> _debugSessionFaulted =
        LoggerMessage.Define<object>(
            LogLevel.Debug,
            4,
            "Session {Session} faulted");

    private static readonly Action<ILogger, object, string, Exception?> _debugCommandExecuting =
        LoggerMessage.Define<object, string>(
            LogLevel.Debug,
            5,
            "Session {Session} got command {Command}");

    public static void Error(this ILogger logger, Exception e)
    {
        _error(logger, e.Message, e);
    }

    public static void DebugSessionCreated(this ILogger logger, SessionEventArgs e)
    {
        var session = e.Context.Properties[EndpointListener.RemoteEndPointKey];
        _debugSessionCreated(logger, session, default);
    }

    public static void DebugSessionCompleted(this ILogger logger, SessionEventArgs e)
    {
        var session = e.Context.Properties[EndpointListener.RemoteEndPointKey];
        _debugSessionCompleted(logger, session, default);
    }

    public static void DebugSessionCancelled(this ILogger logger, SessionEventArgs e)
    {
        var session = e.Context.Properties[EndpointListener.RemoteEndPointKey];
        _debugSessionCancelled(logger, session, default);
    }

    public static void DebugSessionFaulted(this ILogger logger, SessionFaultedEventArgs e)
    {
        var session = e.Context.Properties[EndpointListener.RemoteEndPointKey];
        _debugSessionFaulted(logger, session, default);
    }

    public static void DebugCommandExecuting(this ILogger logger, SmtpCommandEventArgs e)
    {
        var session = e.Context.Properties[EndpointListener.RemoteEndPointKey];

        using var writer = new StringWriter();
        new TracingSmtpCommandVisitor(writer).Visit(e.Command);
        var command = writer.ToString();

        _debugCommandExecuting(logger, session, command, default);
    }
}
