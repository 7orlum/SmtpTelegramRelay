# What is SmtpTelegramRelay

SmtpTelegramRelay is an SMTP server that relays all received emails to specified telegram bot subscribers. Runs as a windows service or as a standalone application. Fully written in C#.

# Setup

1. Edit `appsettings.yaml`. At least specify a telegram bot token and a chat ID.
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
2. Register and run
2.1. Run `SmtpTelegramRelay.exe` as a standalone application
2.2. or register the program as a windows service `sc.exe create "SMTP Telegram Relay" binpath="C:\Program Files\SmtpTelegramRelay\SmtpTelegramRelay.exe" start=auto obj="NT AUTHORITY\LocalService"`
then start the windows service `sc.exe start "SMTP Telegram Relay"`
2.3. or register the program as a systemd service in unix-like operating systems. Create a configuration file `/etc/systemd/system/smtp-telegram-relay.service` looking as follows:
    ```ini
    [Unit]
    Description=SMTP Telegram Relay
    [Service]
    Type=simple
    ExecStart=/usr/sbin/SmtpTelegramRelay
    [Install]
    WantedBy=multi-user.target
    ```
    Then say systemd to load the new configuration file `sudo systemctl daemon-reload` and run the service `sudo systemctl start smtp-telegram-relay.service`

3. Send a test email and get it in telegram. Use `localhost` as an SMTP server address, `25` as a port and no authentifiacation or, if necessary, select the basic authentication method with a fake username and password.
