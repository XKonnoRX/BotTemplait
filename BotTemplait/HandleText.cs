using MySqlX.XDevAPI;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http.Json;
using System.Xml.Linq;
using System.Diagnostics.Contracts;
using Google.Protobuf;
using Telegram.Bot.Types.Payments;
using Microsoft.VisualBasic;
using Google.Protobuf.WellKnownTypes;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Threading;

namespace BotTemplait
{
    internal class HandleText
    {
        private ITelegramBotClient _botClient;
        private long _chatId;
        private int _messageId;
        private int _botmessageId;
        private CancellationToken _cancellationToken;
        private Dictionary<string, string> msgdict;
        public HandleText(ITelegramBotClient botClient, long chatId, int messageId, int botmessageId, CancellationToken cancellationToken)
        {
            _botClient = botClient;
            _chatId = chatId;
            _messageId = messageId;
            _cancellationToken = cancellationToken;
            _botmessageId = botmessageId;
            msgdict = TelegramBot.messageContainer.messageDict;
        }

        public async void Start()
        {
            var reader = DataBase.Read($"SELECT * FROM users WHERE telegramId = {_chatId}");
            if (reader == null)
            {
               Message message = await _botClient.SendTextMessageAsync(
               chatId: _chatId,
               text: $"{msgdict["start"]}\n{msgdict["register"]}",
               parseMode: ParseMode.Html,
               cancellationToken: _cancellationToken);
               string insertQuery = $"INSERT INTO users (telegramId) " +
                                     $"VALUES ('{_chatId}')";
               DataBase.SendCommand(insertQuery);
            }
            else
            {
                Message message = await _botClient.SendTextMessageAsync(
               chatId: _chatId,
               text: msgdict["startsuccess"],
               parseMode: ParseMode.Html,
               replyMarkup:Menu.MainMenu(),
               cancellationToken: _cancellationToken);
               DataBase.Log(TelegramBot.logpath, $"Enter: id: {reader[(int)EnUsers.telegramId]}, name: {message.Chat.Username}");
            }
        }
        public async void Default()
        {
            string msg = "Не смог распознать команду";
            await _botClient.SendTextMessageAsync(
               chatId: _chatId,
               text: msg,
               parseMode: ParseMode.Html,
               cancellationToken: _cancellationToken);
        }
        
    }
}
