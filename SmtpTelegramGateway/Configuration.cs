namespace SmtpTelegramGateway;

internal sealed class Configuration
{
    public ushort SmtpPort { get; set; } = 25;
    public required string TelegramBotToken { get; set; }
    public List<Route> Routing { get; set; } = [];

    internal sealed class Route
    {
        public required string Email { get; set; }
        public required string TelegramChat { get; set; }
    }
}
