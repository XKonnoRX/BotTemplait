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
using System.Linq.Expressions;
using System.Text;
using System.Reflection.PortableExecutable;

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

        public Handle(ITelegramBotClient botClient, Message message, int botmessageId, CancellationToken cancellationToken)
        {
            this.botClient = botClient;
            this.message = message;
            this.chatId = message.Chat.Id;
            this.messageId = message.MessageId;
            this.cancellationToken = cancellationToken;
            this.botmessageId = botmessageId;
            msgdict = TelegramBot.messageContainer.messages;
        }
        public async void Paging(int page)
        {
            //типа есть список
            List<Button> buttons = [];
            for (int i = 0; i < 22; i++)
                buttons.Add(new Button { text = $"Пункт {i + 1}", type = "text", back = $"choose|{i + 1}" });

            //создаем кнопки
            int count = 5;
            var keys = Menu.GenerateInline([.. buttons], page*count, count);
            keys.AddRange(Menu.InlinePages(page, $"page", count, buttons.Count));
            InlineKeyboardMarkup keyboard = new(keys);

            try
            {
                await botClient.EditMessageReplyMarkupAsync(
                    chatId: chatId,
                    messageId: messageId,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken
                    );
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Сделай свой выбор",
                    parseMode: ParseMode.Html,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken
                    );
            }
        }
    }
    public class HandleQuest : Handle
    {
        public QuestContainer Quest { get; set; }
        public HandleQuest((ITelegramBotClient botClient, Message message, int botmessageId, CancellationToken cancellationToken) data, QuestContainer quest)
            : base(data.botClient, data.message, data.botmessageId, data.cancellationToken)
        {
            this.Quest = quest;
        }

        public async void QuestStart()
        {
            var reader = DB.ReadMultiline($"select * from `{Quest.table}` where `user_id` = '{chatId}' and `finish` = 'false'");
            if (reader != null && reader.Length != 0)
                DB.Send($"DELETE FROM `{Quest.table}` WHERE (`id` = '{reader[0][0]}');");
            int localId = 1;
            reader = DB.ReadMultiline($"select * from `{Quest.table}` where `user_id` = '{chatId}'");
            if (reader != null && reader.Length != 0)
                localId = (int)reader[reader.Length - 1][2] +1;
            DB.Send($"INSERT INTO `{Quest.table}` (`user_id`, `local_id`) VALUES ('{chatId}', '{localId}');");
            reader = DB.ReadMultiline($"select * from `{Quest.table}` where `user_id` = '{chatId}'");
            var line = reader[reader.Length - 1];
            await botClient.SendTextMessageAsync(
                chatId:chatId,
                text: Quest.messages["start"],
                parseMode: ParseMode.Html,
                cancellationToken:cancellationToken);
            QuestSend((int)line[0],0);
        }
        public async void QuestSend(int lineId, int questId, bool removePrev = false)
        {
            if(removePrev)
                await botClient.DeleteMessageAsync(
                chatId: chatId,
                messageId: messageId,
                cancellationToken: cancellationToken);
            var quest = Quest.quests[questId];
            IReplyMarkup markup = new InlineKeyboardMarkup(Menu.GenerateInline(quest.buttons, precallback: $"quest|{Quest.name}|{lineId}|{questId}"));
            if (quest.enable_input)
                DB.Update<UserData>(s => s.tg_id == chatId, s => s.bot_state = $"quest|{Quest.name}|{lineId}|{questId}");
            var mess = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: quest.message,
                parseMode: ParseMode.Html,
                replyMarkup: markup,
                cancellationToken: cancellationToken);
            DB.Update<UserData>(s => s.tg_id == chatId, s => s.lastbotmsg = mess.MessageId);
        }
        public async void QuestAnswer(int lineId, int questId, string value)
        {
            var reader = DB.Read($"select * from `{Quest.table}` where `id` = '{lineId}'");
            var quest = Quest.quests[questId];
            await botClient.DeleteMessageAsync(
                chatId: chatId,
                messageId: botmessageId,
                cancellationToken: cancellationToken);
            DB.Send($"Update `{Quest.table}` set `{quest.column}` = '{value}' where id = '{lineId}'");
            if (questId < Quest.quests.Length-1 && !(bool)reader[3])
                QuestSend(lineId, questId + 1);
            else
            {
                DB.Send($"Update `{Quest.table}` set `finish` = true where id = '{lineId}'");
                QuestFinish(lineId);
            }
        }
        public async void QuestFinish(int lineId)
        {
            DB.Update<UserData>(s => s.tg_id == chatId, s => s.bot_state = $"default");
            var builder = new StringBuilder();
            foreach ( var item in Quest.quests )
            {
                builder.Append(item.column);
                builder.Append(", ");
            }
            builder.Remove(builder.Length - 2, 1);
            var reader = DB.Read($"select {builder} from `{Quest.table}` where `id` = '{lineId}'");
            builder = new StringBuilder();
            builder.AppendLine(Quest.messages["finish"]);
            for (var i = 0; i < Quest.quests.Length; i++)
            {
                builder.Append(Quest.quests[i].name);
                builder.Append(": ");
                builder.AppendLine(reader[i].ToString());
            }
            await botClient.SendTextMessageAsync(
                chatId:chatId,
                text: builder.ToString(),
                parseMode:ParseMode.Html,
                replyMarkup:Menu.FinishQuestions(Quest.name, lineId),
                cancellationToken:cancellationToken
                );
        }
        public async void QuestEdit(int lineId)
        {
            var keyboard = new List<InlineKeyboardButton[]>();
            foreach (var item in Quest.quests)
                keyboard.Add([InlineKeyboardButton.WithCallbackData($"Изменить {item.name}",$"qsend|{Quest.name}|{lineId}|{item.id}")]);
            await botClient.EditMessageReplyMarkupAsync(
                chatId: chatId,
                messageId:messageId,
                replyMarkup: new(keyboard),
                cancellationToken: cancellationToken
                );
        }
        public async void QuestOpen(int lineId)
        {
            await botClient.DeleteMessageAsync(
                chatId: chatId,
                messageId: messageId,
                cancellationToken: cancellationToken);
            var builder = new StringBuilder();
            foreach (var item in Quest.quests)
            {
                builder.Append(item.column);
                builder.Append(", ");
            }
            builder.Remove(builder.Length - 2, 1);
            var reader = DB.Read($"select {builder} from `{Quest.table}` where `id` = '{lineId}'");
            builder = new StringBuilder();
            builder.AppendLine(Quest.messages["open"]);
            for (var i = 0; i < Quest.quests.Length; i++)
            {
                builder.Append(Quest.quests[i].name);
                builder.Append(": ");
                builder.AppendLine(reader[i].ToString());
            }
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: builder.ToString(),
                parseMode: ParseMode.Html,
                replyMarkup: Menu.EditQuestions(Quest.name, lineId),
                cancellationToken: cancellationToken
                );
        }
        public async void QuestList()
        {
            var reader = DB.ReadMultiline($"select * from `{Quest.table}` where `user_id` = '{chatId}' && `finish` = true");
            var keyboard = new List<InlineKeyboardButton[]>();
            foreach (var item in reader)
                keyboard.Add([InlineKeyboardButton.WithCallbackData($"{Quest.title}: {item[2]}", $"qopen|{Quest.name}|{item[0]}")]);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: Quest.messages["list"],
                parseMode: ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(keyboard),
                cancellationToken: cancellationToken
                );
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
