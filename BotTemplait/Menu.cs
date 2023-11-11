using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplait
{
    internal class Menu
    {
        public static KeyboardButton[][] GenerateMenu(string[] parametrs, int rows)
        {

            if (rows <= parametrs.Length)
            {
                var keyboard = new List<KeyboardButton[]>();
                int count = parametrs.Length / rows;
                if (parametrs.Length % rows != 0)
                    count++;
                for (int i = 0; i < count; i++)
                {
                    var buttons = new List<KeyboardButton>();

                    for (int t = 0; t < rows; t++)
                        try
                        {
                            buttons.Add(new KeyboardButton(parametrs[i * rows + t]));
                        }
                        catch { }
                    keyboard.Add(buttons.ToArray());
                }
                return keyboard.ToArray();
            }
            else
            {
                var keyboard = new List<KeyboardButton[]>();
                for (int i = 0; i < parametrs.Length; i++)
                {
                    keyboard.Add(new KeyboardButton[] { parametrs[i] });
                }
                return keyboard.ToArray();
            }
        }
        public static string[] GetMenuButtons(string name)
        {
            var menues = TelegramBot.messageContainer.menues;
            foreach (var menu in menues)
            {
                if (menu.name == name)
                    return menu.buttons;
            }
            return null;
        }
        public static Button[] GetInlineButtons(string name)
        {
            var inlines = TelegramBot.messageContainer.inlines;
            foreach (var inline in inlines)
            {
                if (inline.name == name)
                    return inline.buttons;
            }
            return null;
        }
        public static List<InlineKeyboardButton[]> GenerateInline(Button[] buttons, string callback, int start, int count)
        {
            var lenght = buttons.Length - start;
            if (lenght > count)
                lenght = count;
            var keyboard = new List<InlineKeyboardButton[]>();
            for (int i = start; i < start + lenght; i++)
            {
                switch (buttons[i].type)
                {
                    case "text":
                        keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: buttons[i].text, callbackData: $"{buttons[i].back}|{callback}") });
                        break;
                    case "url":
                        keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: buttons[i].text, url: buttons[i].back) });
                        break;
                    case "payment":
                        keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithPayment(text: buttons[i].text) });
                        break;
                }
            }
            return keyboard;
        }
        public static List<InlineKeyboardButton[]> GenerateInline(Button[] buttons, string callback)
        {
            return GenerateInline(buttons, callback, 0, buttons.Length);
        }
        public static List<InlineKeyboardButton[]> GenerateInline(Button[] buttons)
        {
            return GenerateInline(buttons, "", 0, buttons.Length);
        }
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int text, int callback, int active, int start, int count)
        {
            var lenght = buttons.Length - start;
            if (lenght > count)
                lenght = count;
            var keyboard = new List<InlineKeyboardButton[]>();
            for (int i = start; i < start + lenght; i++)
                if (buttons[i][active].ToString() == "1")
                    keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: buttons[i][text].ToString(), callbackData: buttons[i][callback].ToString()) });
            return keyboard;
        }
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int text, int callback, int active)
        {
            return GenerateInlineFromDatabase(buttons, text, callback, active, 0, buttons.Length);
        }
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int text, int callback, int start, int count)
        {
            var lenght = buttons.Length - start;
            if (lenght > count)
                lenght = count;
            var keyboard = new List<InlineKeyboardButton[]>();
            for (int i = start; i < start + lenght; i++)
                keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: buttons[i][text].ToString(), callbackData: buttons[i][callback].ToString()) });
            return keyboard;
        }
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int[] text, string precallback, int callback, int start, int count)
        {
            var lenght = buttons.Length - start;
            if (lenght > count)
                lenght = count;
            var keyboard = new List<InlineKeyboardButton[]>();
            for (int i = start; i < start + lenght; i++)
            {
                string fulltext = "";
                foreach (var t in text)
                    fulltext += buttons[i][t].ToString() + " ";
                keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: fulltext, callbackData: $"{precallback}|{buttons[i][callback]}") });
            }
                
            return keyboard;
        }
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int text, int callback)
        {
            return GenerateInlineFromDatabase(buttons, text, callback, 0, buttons.Length);
        }
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int[] text, string precallback, int callback)
        {
            return GenerateInlineFromDatabase(buttons, text, precallback, callback, 0, buttons.Length);
        }
        public static List<InlineKeyboardButton[]> InlinePages(int page, string callback, bool back, bool front)
        {
            string p = IntegerToEmoji(page + 1);
            if (front && back)
                return new List<InlineKeyboardButton[]>() { new InlineKeyboardButton[] {
                    InlineKeyboardButton.WithCallbackData(text: "⬅️", callbackData: $"{callback}|{page - 1}"),
                    InlineKeyboardButton.WithCallbackData(text: $"{p}", callbackData: $"{callback}|{page}"),
                    InlineKeyboardButton.WithCallbackData(text: "➡️", callbackData: $"{callback}|{page + 1}")
                } };
            if (front)
                return new List<InlineKeyboardButton[]>() { new InlineKeyboardButton[] {
                    InlineKeyboardButton.WithCallbackData(text: $"{p}", callbackData: $"{callback}|{page}"),
                    InlineKeyboardButton.WithCallbackData(text: "➡️", callbackData: $"{callback}|{page + 1}")
                } };
            if (back)
                return new List<InlineKeyboardButton[]>() { new InlineKeyboardButton[] {
                    InlineKeyboardButton.WithCallbackData(text: "⬅️", callbackData: $"{callback}|{page - 1}"),
                    InlineKeyboardButton.WithCallbackData(text: $"{p}", callbackData: $"{callback}|{page}")
                } };
            return null;

        }
        public static string IntegerToEmoji(int number)
        {
            string res = "";
            foreach(var digit in number.ToString())
            {
                switch(int.Parse(digit.ToString()))
                {
                    case 0:
                        res += "0️⃣";
                        break;
                    case 1:
                        res += "1️⃣";
                        break;
                    case 2:
                        res += "2️⃣";
                        break;
                    case 3:
                        res += "3️⃣";
                        break;
                    case 4:
                        res += "4️⃣";
                        break;
                    case 5:
                        res += "5️⃣";
                        break;
                    case 6:
                        res += "6️⃣";
                        break;
                    case 7:
                        res += "7️⃣";
                        break;
                    case 8:
                        res += "8️⃣";
                        break;
                    case 9:
                        res += "9️⃣";
                        break;
                }
            }
            return res;
        }


        //Examples
        public static ReplyKeyboardMarkup MainMenu()
        {
            return new(GenerateMenu(GetMenuButtons("main"), 2)) { ResizeKeyboard = true, IsPersistent = true };
        }
        public static InlineKeyboardMarkup Cars()
        {
            var reader = DataBase.ReaderMultiline("SELECT * FROM cars WHERE Status = 'Active'");
            return new(GenerateInlineFromDatabase(reader, new int[] { 1, 0 }, "car", 0));
        }
        public static InlineKeyboardMarkup WorkPlace()
        {
            return new(GenerateInline(GetInlineButtons("work_place")));
        }
    }
}
