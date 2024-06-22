using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Xml.Linq;
using System.Diagnostics.Contracts;
using Telegram.Bot.Types.Payments;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace BotTemplait
{
    public class Handle
    {
        protected ITelegramBotClient botClient;
        protected long chatId;
        protected int messageId;
        protected Message message;
        protected int botmessageId;
        protected CancellationToken cancellationToken;
        protected Dictionary<string, string> msgdict;
        protected Quest[] quests; 

        public Handle(ITelegramBotClient botClient, Message message, int botmessageId, CancellationToken cancellationToken)
        {
            this.botClient = botClient;
            this.message = message;
            this.chatId = message.Chat.Id;
            this.messageId = message.MessageId;
            this.cancellationToken = cancellationToken;
            this.botmessageId = botmessageId;
            msgdict = TelegramBot.messageContainer.messages;
            quests = TelegramBot.quests;
        }
        public async void QuestStart()
        {
            //DB.Insert<>();
            QuestSend(0);
        }
        public async void QuestSend(int questId)
        {
            var quest = quests[questId];
            await botClient.SendTextMessageAsync(
                chatId:chatId,
                text: quest.message,
                parseMode:ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(Menu.GenerateInline(quest.buttons)),
                cancellationToken:cancellationToken);
        }
        public async void QuestAnswer(int questId, string value)
        {
            var quest = quests[questId];
            await botClient.DeleteMessageAsync(
                chatId: chatId,
                messageId: messageId,
                cancellationToken: cancellationToken);
            DB.Send($"Update `Table` set `{quest.column}` = {value} where tg_id = {chatId}");
            if (questId < quests.Length)
                QuestSend(questId + 1);
            else
                QuestFinish();
        }
        public async void QuestFinish()
        {
            
        }
    }
    public class HandleText:Handle
    {
        public HandleText(ITelegramBotClient botClient, Message message, int botmessageId, CancellationToken cancellationToken) 
            : base(botClient, message, botmessageId, cancellationToken)
        {
        }

        public async void Start()
        {
            var user = DB.Find<UserData>(s=>s.tg_id == chatId);
            if (user == null)
            {
                var newUser = new UserData();
                newUser.tg_id = chatId;
                newUser.username = message.Chat.Username;
                newUser.firstname = message.Chat.FirstName;
                newUser.lastname = message.Chat.LastName;
                DB.Insert<UserData>(newUser);
            }
            await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: msgdict["start"],
               parseMode: ParseMode.Html,
               replyMarkup:Menu.MainMenu(),
               cancellationToken: cancellationToken);
            Console.WriteLine($"Enter: id: {chatId}, name: {message.Chat.Username}");
        }

        public async void Default()
        {
            string msg = "Не смог распознать команду";
            await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: msg,
               parseMode: ParseMode.Html,
               cancellationToken: cancellationToken);
        }
    }
    public class HandleCallback : Handle
    {
        private CallbackQuery callbackQuery;
        private string callbackData;
        public HandleCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, Message message, int botmessageId, CancellationToken cancellationToken)
            : base(botClient, message, botmessageId, cancellationToken)
        {
            this.callbackQuery = callbackQuery;
            callbackData = callbackQuery.Data;
        }
    }
    public class HandleState : HandleText
    {
        public HandleState(ITelegramBotClient botClient, Message message, int botmessageId, CancellationToken cancellationToken) 
            : base(botClient, message, botmessageId, cancellationToken)
        {
        }
    }
}
