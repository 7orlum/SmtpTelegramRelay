using System.Configuration;

namespace SmtpTelegramRelay;

sealed class SmtpConfiguration : System.Configuration.ConfigurationSection
{
    [ConfigurationProperty("port", DefaultValue = 25)]
    public int Port => (int)this["port"];

    public static SmtpConfiguration Read()
    {
        return (SmtpConfiguration)System.Configuration.ConfigurationManager.GetSection("smtpSettings") ?? new SmtpConfiguration();
    }
}
