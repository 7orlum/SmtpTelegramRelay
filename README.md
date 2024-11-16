# What is SmtpTelegramGateway

SmtpTelegramGateway is an SMTP gateway that forwards received emails to specified Telegram chats via your Telegram bot. Runs as a windows service, as a unix daemon or as a standalone application. Fully written in C#.

# Setup

1. Edit `appsettings.yaml`. At least specify a telegram bot token and a chat ID.
    ```yaml
    # The port that the gateway will listen on to receive SMTP e-mail messages, the default is 25. 
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
    # Logging Level. Set to Debug to see the details of the communication between your mail program and the gateway.
    # Set to Error to see less information
    Logging:
      LogLevel:
        Default: Debug
    ```
2. Register and run  
    a. Run `SmtpTelegramGateway.exe` as a standalone application  
    b. Or register the program as a windows service `sc.exe create "SMTP Telegram Gateway" binpath="C:\Program Files\SmtpTelegramGateway\SmtpTelegramGateway.exe" start=auto obj="NT AUTHORITY\LocalService"`
then start the windows service `sc.exe start "SMTP Telegram Gateway"`  
    c. Or register the program as a systemd service in unix-like operating systems. Create a configuration file `/etc/systemd/system/smtp-telegram-gateway.service` looking as follows:
        ```ini
        [Unit]
        Description=SMTP Telegram Gateway
        [Service]
        Type=simple
        ExecStart=/usr/sbin/SmtpTelegramGateway
        [Install]
        WantedBy=multi-user.target
        ```
        Then say systemd to load the new configuration file `sudo systemctl daemon-reload` and run the service `sudo systemctl start smtp-telegram-gateway.service`

3. Send a test email and get it in telegram. Use `localhost` as an SMTP server address, `25` as a port and no authentifiacation or, if necessary, select the basic authentication method with a fake username and password.
