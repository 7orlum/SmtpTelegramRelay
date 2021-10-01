using MimeKit;
using SmtpServer;
using SmtpServer.Storage;
using SmtpServer.Protocol;
using System.Buffers;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace SmtpTelegramRelay
{
    class TelegramAsMessageStore : MessageStore
    {
        public TelegramAsMessageStore(string token, int chatId, IWebProxy proxy)
        {
            if (proxy == null)
                _bot = new TelegramBotClient(token);
            else
                _bot = new TelegramBotClient(token, proxy);

            _chatId = chatId;
        }

        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var stream = new MemoryStream(buffer.ToArray(), writable: false);
            var message = await MimeMessage.LoadAsync(stream, cancellationToken);

            await _bot.SendTextMessageAsync(
                chatId: _chatId, 
                text: $"{message.Subject}\nFrom: {message.From}\nTo: {message.To}\n{message.TextBody}", 
                cancellationToken: cancellationToken);

            return SmtpResponse.Ok;
        }

        TelegramBotClient _bot;
        int _chatId;
    }
}