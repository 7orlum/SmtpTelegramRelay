using System.Configuration;


namespace SmtpTelegramRelay
{
    class TelegramConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("token")]
        public string Token => (string)this["token"];


        [ConfigurationProperty("chatId")]
        public int ChatId => (int)this["chatId"];


        [ConfigurationProperty("proxy", DefaultValue = null)]
        public string Proxy => (string)this["proxy"];


        public static TelegramConfiguration Read()
        {
            return (TelegramConfiguration)System.Configuration.ConfigurationManager.GetSection("telegramSettings") ?? new TelegramConfiguration();
        }
    }
}
