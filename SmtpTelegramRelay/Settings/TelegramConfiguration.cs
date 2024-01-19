using System.Configuration;
using System.ComponentModel;

namespace SmtpTelegramRelay;

sealed class TelegramConfiguration : System.Configuration.ConfigurationSection
{
    [ConfigurationProperty("token")]
    public string Token => (string)this["token"];

    [ConfigurationProperty("chatId")]
    public int ChatId => (int)this["chatId"];

    [ConfigurationProperty("proxy", DefaultValue = null)]
    [TypeConverter(typeof(ProxySettingsConverter))]
    public ProxySettings Proxy => (ProxySettings)this["proxy"];

    public static TelegramConfiguration Read()
    {
        return (TelegramConfiguration)System.Configuration.ConfigurationManager.GetSection("telegramSettings") ?? new TelegramConfiguration();
    }
}
