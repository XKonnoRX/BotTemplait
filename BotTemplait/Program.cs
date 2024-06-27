using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading;
using System.Text.RegularExpressions;
using Serilog;
using Newtonsoft.Json;

namespace BotTemplait
{
    class TelegramBot
    {
        private static TelegramBotClient Client;

        public static MessageContainer messageContainer;
        public static Config config;
        static void Main()
        {
            config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText("config.json"));
            messageContainer = JsonConvert.DeserializeObject<MessageContainer>(System.IO.File.ReadAllText($"messages-ru-ru.json"));
            string connectionString = config.mysql;
            DB.connectionString = connectionString;
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            var tables = DB.ReadMultiline("SHOW TABLES;");
            if (tables.Length == 0)
                DB.CreateTables();
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

            Log.Information("Bot ID: {Id}. Bot NAME: {Username}", me.Id, me.Username);
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
        public static async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Log.Error(ErrorMessage);
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
            var split = callbackData.Split("|");
            HandleCallback handle = new HandleCallback(botClient, callbackQuery, message, messageId, cancellationToken);
            Log.Information("Callback {callbackData} from {chatId}", callbackData, chatId);
            var qhandle = (botClient, message, messageId, cancellationToken);
            var questHandlers = new Dictionary<string, HandleQuest>
            {
                {"quest1", new HandleQuest(qhandle, JsonConvert.DeserializeObject<QuestContainer>(System.IO.File.ReadAllText("quest1.json"))) }
            };
            var commandHandlers = new Dictionary<string, Action>
            {
                {"qsend", () => questHandlers[split[1]].QuestSend(int.Parse(split[2]), int.Parse(split[3]), true) },
                {"quest", () => questHandlers[split[1]].QuestAnswer(int.Parse(split[2]), int.Parse(split[3]), split[4]) },
                {"qedit", () => questHandlers[split[1]].QuestEdit(int.Parse(split[2])) },
                {"qopen", () => questHandlers[split[1]].QuestOpen(int.Parse(split[2])) },
                {"qconf", () => botClient.DeleteMessageAsync(chatId: chatId,messageId: messageId,cancellationToken: cancellationToken) }
            };
            if (commandHandlers.ContainsKey(split[0]))
            {
                commandHandlers[split[0]].Invoke();
                return;
            }
        }
        public static async Task HandleText(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;
            long chatId = message.Chat.Id;
            string messageText = message.Text;
            var user = DB.Find<UserData>(s => s.tg_id == message.Chat.Id);
            int msgId = 0;
            if (user != null)
                msgId = user.lastbotmsg;
            HandleText handle = new HandleText(botClient, message, msgId, cancellationToken);
            Log.Information("Message '{messageText}' message in chat {chatId}.", messageText, chatId);
            var qhandle = (botClient, message, msgId, cancellationToken);
            var questHandlers = new Dictionary<string, HandleQuest>
            {
                {"quest1", new HandleQuest(qhandle, JsonConvert.DeserializeObject<QuestContainer>(System.IO.File.ReadAllText("quest1.json"))) }
            };
            var commandHandlers = new Dictionary<string, Action>
            {
                {"/start", () => handle.Start() },
                {"Пройти опрос", () => questHandlers["quest1"].QuestStart()},
                {"Посмотреть опросы", () => questHandlers["quest1"].QuestList()},
            };
            if (user == null)
            {
                handle.Start();
                return;
            }
            if (commandHandlers.ContainsKey(message.Text))
            {
                commandHandlers[message.Text].Invoke();
                if (user != null)
                    DB.Update<UserData>(s => s.tg_id == user.tg_id, s => s.bot_state = "default");
                return;
            }

            var split = user.bot_state.Split("|");
            var stateHandlers = new Dictionary<string, Action>
            {
                {"quest", () => questHandlers[split[1]].QuestAnswer(int.Parse(split[2]), int.Parse(split[3]), messageText) }
            };
            if (stateHandlers.ContainsKey(split[0]))
            {
                stateHandlers[split[0]].Invoke();
                return;
            }
            return;

        }
    }
}