using MySqlX.XDevAPI;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http.Json;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading;
using System.Text.RegularExpressions;

namespace BotTemplait
{
    class TelegramBot
    {
        private static TelegramBotClient Client;

        public static MessageContainer messageContainer;
        public static string logpath = "";
        public static Config config;
        static void Main()
        {
            config = JsonSerializer.Deserialize<Config>(System.IO.File.ReadAllText("config.json"));
            var date = DateTime.Now;
            System.IO.Directory.CreateDirectory("logs");
            logpath = $"logs/{date.ToString("yyyy.MM.dd HH.mm.ss")} log.txt";
            System.IO.File.Create(logpath);
            messageContainer = JsonSerializer.Deserialize<MessageContainer>(System.IO.File.ReadAllText($"config/messages-ru-ru.json"));
            messageContainer.CreateDictionary();
            string connectionString = config.mysql;
            DataBase.connectionString = connectionString;
            var tables = DataBase.ReadMultiline("SHOW TABLES;");
            if (tables.Length == 0)
                DataBase.Send("CREATE TABLE `users` (`telegramId` VARCHAR(45) NOT NULL,`lastbotmsg` INT NULL DEFAULT 0, PRIMARY KEY (`telegramId`));");
            Client = new TelegramBotClient(config.telegramToken);
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(), // receive all update types
            };
            Client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );
            var me = Client.GetMeAsync().Result;
            DataBase.Log(logpath, $"Bot ID: {me.Id}. Bot NAME: {me.FirstName}");
            Thread.Sleep(-1);
        }
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
            {
                await HandleText(botClient, update, cancellationToken);
                return;
            }
            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update, cancellationToken);
                return;
            }
            return;

        }
        public static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            DataBase.Log(logpath, ErrorMessage);
            Process.Start("BotTemplait");
            Environment.Exit(0);
            return Task.CompletedTask;
        }
        public static async Task HandleCallbackQuery(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery is not { } callbackQuery)
                return;
            if (callbackQuery.Data is not { } callbackData)
                return;
            long chatId = callbackQuery.Message.Chat.Id;
            int messageId = callbackQuery.Message.MessageId;
            var message = callbackQuery.Message;
            callbackData = callbackQuery.Data;
            HandleText customHandleText = new HandleText(botClient, message, messageId, cancellationToken);
            DataBase.Log(logpath, $"Callback {callbackData} from {chatId}");
            try
            {
                
            }
            catch
            {}
        }
        public static async Task HandleText(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;
            string messageText = message.Text;
            var reader = DataBase.Read($"SELECT * FROM users WHERE telegramId = {message.Chat.Id}");
            int msgId = 0;
            if (reader != null)
                msgId = int.Parse(reader[(int)EnUsers.lastbotmsg].ToString());
            HandleText customHandleText = new HandleText(botClient, message, msgId, cancellationToken);
            DataBase.Log(logpath, $"Message '{message.Text}' message in chat {message.Chat.Id}.");
            
            if (reader == null)
            {
                customHandleText.Start();
                return;
            }
            try
            {
                
            }
            catch
            {
                customHandleText.Default();
            }
            return;

        }
        public static void CheckLog(string logpath)
        {
            if (logpath == null) return;
            int readed = 0;
            while (true)
            {
                var text = System.IO.File.ReadAllLines(logpath);
                for(int i = readed; i< text.Length; i++)
                {
                    Console.WriteLine(text[i]);
                    readed = i+1;
                }
                Thread.Sleep(500);
            }
        }

    }
}