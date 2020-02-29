using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.ComponentModel;


namespace SmtpTelegramRelay
{
    enum ProxyType { None, HTTP };


    class ProxySettings
    {
        public ProxyType ProxyType;
        public string UserName;
        public string Password;
        public string Address;
        public int Port;


        public ProxySettings(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                ProxyType = ProxyType.None;
                return;
            }

            //parse string user:password@address:port
            var regex = new Regex(@"^((?<username>[^: ]+):(?<password>.+)@)?(?<address>[^: ]+)(:(?<port>\d{1,5}))?$", RegexOptions.Singleline);
            var match = regex.Match(s.Trim());
            if (match.Success)
            {
                ProxyType = ProxyType.HTTP;
                UserName = match.Groups["username"].Value;
                Password = match.Groups["password"].Value;
                Address = match.Groups["address"].Value;

                if (string.IsNullOrWhiteSpace(match.Groups["port"].Value))
                {
                    Port = 80;
                    return;
                }

                if (int.TryParse(match.Groups["port"].Value, out Port))
                    return;
            }

            throw new ArgumentException($"Can't parse proxy string {s}");
        }


        public IWebProxy GetIWebProxy()
        {
            switch (ProxyType)
            {
                case ProxyType.None:
                    return null;
                case ProxyType.HTTP:
                    if (string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password))
                        return new WebProxy(Address, Port);
                    else
                        return new WebProxy(Address, Port) { Credentials = new NetworkCredential(UserName, Password) };
                default:
                    throw new NotImplementedException();
            }
        }


        public override string ToString()
        {
            if (ProxyType == ProxyType.None)
                return string.Empty;

            var result = new StringBuilder($"{ProxyType.ToString().ToLowerInvariant()}//:");

            if (!string.IsNullOrEmpty(UserName) || !string.IsNullOrEmpty(Password))
                result.Append($"{UserName}:{Password}");

            result.Append($"{Address}:{Port}");

            return result.ToString();
        }
    }


    public class ProxySettingsConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new ProxySettings((string)value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return value.ToString();
        }
    }
}
