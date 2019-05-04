using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Storage;
using SmtpServer.Protocol;
using SmtpServer.Mail;
using MimeKit;
using Telegram.Bot;


namespace SmtpTelegramRelay
{
    internal class TelegramAsMessageStore : MessageStore
    {
        public TelegramAsMessageStore(string token, int chatId)
        {
            _bot = new TelegramBotClient(token);
            _chatId = chatId;
        }


        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
        {
            var message = MimeMessage.Load(((ITextMessage)transaction.Message).Content);

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