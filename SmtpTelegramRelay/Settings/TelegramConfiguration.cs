using System.Configuration;


namespace SmtpTelegramRelay
{
    internal class TelegramConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("token")]
        public string Token => (string)this["token"];


        [ConfigurationProperty("chatId")]
        public int ChatId => (int)this["chatId"];


        public static TelegramConfiguration Read()
        {
            return (TelegramConfiguration)System.Configuration.ConfigurationManager.GetSection("telegramSettings") ?? new TelegramConfiguration();
        }
    }
}
