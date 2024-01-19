using MimeKit;
using SmtpServer;
using SmtpServer.Storage;
using SmtpServer.Protocol;
using System.Buffers;
using Telegram.Bot;

namespace SmtpTelegramRelay
{
    sealed class Store : MessageStore
    {
        TelegramBotClient _bot;
        int _chatId;

        public Store(string token, int chatId)
        {
            _bot = new TelegramBotClient(token);
            _chatId = chatId;
        }

        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream(buffer.ToArray(), writable: false);
            var message = await MimeMessage.LoadAsync(stream, cancellationToken).ConfigureAwait(false);
            var text = $"{message.Subject}\nFrom: {message.From}\nTo: {message.To}\n{message.TextBody}";
            await _bot.SendTextMessageAsync(_chatId, text, cancellationToken: cancellationToken).ConfigureAwait(false);
            return SmtpResponse.Ok;
        }
    }
}