# What is SmtpTelegramRelay

SmtpTelegramRelay is an SMTP server that relays all received emails to specified telegram bot subscribers. Runs as a windows service or as a standalone application. Fully written in C#.

# Setup

Edit `appsettings.yaml`. First, specify your telegram bot token and chatId.
```yaml
# The port that the relay will listen on to receive SMTP e-mail messages, the default is 25. 
# No authorization is required when connecting to this port, select Basic Authorizathion if it is required
SmtpPort: 25
# Your token for the Telegram bot, get it at https://t.me/BotFather when registering the bot
TelegramBotToken: SPECIFY THERE TELEGRAM BOT TOKEN
# Define here a list of email addresses and telegram chats that will receive emails sent to these addresses.
# Use an asterisk "*" instead of an email address to send all emails to some telegram chat
Routing:
-   Email: "*"
    TelegramChatId: SPECIFY THERE TELEGRAM CHATID
-   Email: example@test.com
    TelegramChatId: SPECIFY THERE TELEGRAM CHATID
# Logging Level. Set to Debug to see the details of the communication between your mail program and the relay.
# Set to Error to see less information
Logging:
  LogLevel:
    Default: Debug
```
  
* Run `SmtpTelegramRelay.exe` as a standalone application
* or register the program as a Windows Service `sc.exe create "Smtp Telegram Relay" binpath="C:\Program Files\SmtpTelegramRelay\SmtpTelegramRelay.exe"`
and start it `sc.exe start "Smtp Telegram Relay"`
  
Send a test email and get it in telegram. Use `localhost` as an SMTP server address, `25` as a port and no authentifiacation or, if necessary, select the basic authentication method with a fake username and password.
