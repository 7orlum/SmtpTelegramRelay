namespace SmtpTelegramGateway;

internal sealed class Configuration
{
    public ushort SmtpPort { get; set; } = 25;
    public string TelegramBotToken { get; set; } = default!;
    public List<Route> Routing { get; set; } = [];

    internal sealed class Route
    {
        public string Email { get; set; } = default!;
        public long TelegramChatId { get; set; }
    }
}
