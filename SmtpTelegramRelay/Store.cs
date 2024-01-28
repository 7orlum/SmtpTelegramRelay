using MimeKit;
using SmtpServer;
using SmtpServer.Storage;
using SmtpServer.Protocol;
using System.Buffers;
using Telegram.Bot;
using Microsoft.Extensions.Options;

namespace SmtpTelegramRelay;

internal sealed class Store(IOptionsMonitor<RelayConfiguration> options) : MessageStore
{
    private const string _asterisk = "*";
    private TelegramBotClient? _bot;
    private string? _token;

    public override async Task<SmtpResponse> SaveAsync(
        ISessionContext context, 
        IMessageTransaction transaction, 
        ReadOnlySequence<byte> buffer, 
        CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(buffer.ToArray(), writable: false);
        var message = await MimeMessage.LoadAsync(stream, cancellationToken).ConfigureAwait(false);
        var text = $"{message.Subject}\nFrom: {message.From}\nTo: {message.To}\n{message.TextBody}";

        var currentOptions = options.CurrentValue;
        PrepareBot(currentOptions, cancellationToken);
        foreach (var chat in GetChats(currentOptions, message.To))
        {
            await _bot!.SendTextMessageAsync(chat, text, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        
        return SmtpResponse.Ok;
    }

    private void PrepareBot(RelayConfiguration currentOptions, CancellationToken cancellationToken)
    {
        if (_bot != null && _token != currentOptions.TelegramBotToken)
        {
            _ = _bot.CloseAsync(cancellationToken);
            _bot = null;
        }

        if (_bot == null)
        {
            _bot = new TelegramBotClient(currentOptions.TelegramBotToken);
            _token = currentOptions.TelegramBotToken;
        }
    }

    private static IEnumerable<int> GetChats(RelayConfiguration options, InternetAddressList emails)
    {
        var result = new List<int>();
        
        foreach (var address in emails) 
        {
            switch (address)
            {
                case MailboxAddress email:
                    var chats = options.Routing
                        .Where(r => String.Equals(r.Email, email.Address, StringComparison.OrdinalIgnoreCase))
                        .Select(r => r.TelegramChatId)
                        .ToArray();

                    if (chats.Length == 0)
                    {
                        chats = options.Routing
                            .Where(r => r.Email == _asterisk)
                            .Select(r => r.TelegramChatId)
                            .ToArray();
                    }

                    result.AddRange(chats);
                    break;
                case GroupAddress group:
                    result.AddRange(GetChats(options, group.Members));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return result.Distinct();
    }
}
