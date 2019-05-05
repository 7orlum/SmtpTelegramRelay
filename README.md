# What is SmtpTelegramRelay?

SmtpTelegramRelay is an SMTP server that relays all received emails to the specified telegram bot subscriber. Works as a windows service or as a standalone application. Completely written in C#.

# How can it be used?

Specify your telegram bot token and chatId in `SmtpTelegramRelay.exe.config`
```xml
  <telegramSettings token="SPECIFY THERE TELEGRAM BOT TOKEN" chatId="SPECIFY THERE TELEGRAM CHATID" />`
  <smtpSettings port="25" />
```
  
Start SMTP Telegram Relay as a service or as a standalone application:
* `SmtpTelegramRelay.exe` without parameters runs SMTP Telegram Relay as a standalone application.  
* `SmtpTelegramRelay.exe install` installs SMTP Telegram Relay as a service. Don't forgive to manually start the service after installation.  
* `SmtpTelegramRelay.exe uninstall` uninstalls the SMTP Telegram Relay service.  
  
Send a test email through your SMTP Telegram Relay. Use `localhost` as an SMTP server address, `25` as a port and no authentifiacation or basic authentification method with fake username and password.
