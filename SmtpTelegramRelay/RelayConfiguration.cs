namespace SmtpTelegramRelay;

internal sealed class RelayConfiguration
{
    public ushort SmtpPort { get; set; } = 25;
    public string TelegramBotToken { get; set; } = default!;
    public List<Route> Routing { get; set; } = new List<Route>();

    public sealed class Route
    {
        public string Email { get; set; } = default!;
        public int TelegramChatId { get; set; }
    }
}
