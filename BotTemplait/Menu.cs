using System.Linq.Expressions;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplait
{
    internal class Menu
    {
        /// <summary>
        /// Генерирует меню клавиатуры для телеграм-бота на основе переданных параметров.
        /// </summary>
        /// <param name="parametrs">Массив строк, представляющих кнопки меню.</param>
        /// <param name="rows">Количество строк в клавиатуре.</param>
        /// <returns>Двумерный массив кнопок клавиатуры для телеграм-бота.</returns>
        public static KeyboardButton[][] GenerateMenu(string[] parametrs, int rows)
        {
            // Проверяем, что количество строк не превышает общее количество параметров
            if (rows <= parametrs.Length)
            {
                var keyboard = new List<KeyboardButton[]>();
                int count = parametrs.Length / rows;

                // Вычисляем количество необходимых строк для размещения всех параметров
                if (parametrs.Length % rows != 0)
                    count++;

                // Создаем строки клавиатуры и добавляем кнопки
                for (int i = 0; i < count; i++)
                {
                    var buttons = new List<KeyboardButton>();

                    for (int t = 0; t < rows; t++)
                        try
                        {
                            // Добавляем кнопки в текущую строку клавиатуры
                            buttons.Add(new KeyboardButton(parametrs[i * rows + t]));
                        }
                        catch { }

                    // Добавляем строку клавиатуры в результат
                    keyboard.Add(buttons.ToArray());
                }
                return keyboard.ToArray();
            }
            else
            {
                // Если количество строк больше, чем общее количество параметров, создаем однокнопочные строки
                var keyboard = new List<KeyboardButton[]>();
                for (int i = 0; i < parametrs.Length; i++)
                {
                    // Добавляем кнопку в отдельную строку клавиатуры
                    keyboard.Add(new KeyboardButton[] { parametrs[i] });
                }
                return keyboard.ToArray();
            }
        }
        /// <summary>
        /// Генерирует встроенные кнопки клавиатуры для телеграм-бота на основе переданных параметров.
        /// </summary>
        /// <param name="buttons">Массив объектов кнопок.</param>
        /// <param name="postcallback">Callback-данные, общие для всех кнопок.</param>
        /// <param name="start">Начальный индекс кнопки в массиве.</param>
        /// <param name="count">Количество кнопок, которые следует включить в результат.</param>
        /// <returns>Список массивов встроенных кнопок для телеграм-бота.</returns>
        public static List<InlineKeyboardButton[]> GenerateInline(Button[] buttons,  int start, int count, string precallback = "", string postcallback = "")
        {
            var length = buttons.Length - start;
            if (length > count)
                length = count;

            var keyboard = new List<InlineKeyboardButton[]>();
            for (int i = start; i < start + length; i++)
            {
                switch (buttons[i].type)
                {
                    case "text":
                        // Добавляем встроенную кнопку с текстовыми callback-данными
                        keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: buttons[i].text, callbackData: $"{buttons[i].back}|{postcallback}") });
                        break;
                    case "url":
                        // Добавляем встроенную кнопку с URL-адресом
                        keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: buttons[i].text, url: buttons[i].back) });
                        break;
                    case "payment":
                        // Добавляем встроенную кнопку для оплаты
                        keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithPayment(text: buttons[i].text) });
                        break;
                    case "webapp":
                        // Добавляем встроенную кнопку c webapp
                        var webinfo = new WebAppInfo();
                        webinfo.Url = buttons[i].back;
                        keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithWebApp(text: buttons[i].text, webAppInfo: webinfo) });
                        break;
                }
            }
            return keyboard;
        }
        /// <summary>
        /// Генерирует встроенные кнопки клавиатуры для телеграм-бота на основе переданных параметров.
        /// </summary>
        /// <param name="buttons">Массив объектов кнопок.</param>
        /// <param name="postcallback">Callback-данные, общие для всех кнопок.</param>
        /// <returns>Список массивов встроенных кнопок для телеграм-бота.</returns>
        public static List<InlineKeyboardButton[]> GenerateInline(Button[] buttons, string precallback = "", string postcallback = "")
        {
            // Вызываем первую версию метода с начальным индексом 0 и количеством кнопок равным длине массива
            return GenerateInline(buttons, 0, buttons.Length, precallback, postcallback);
        }
        
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase<T>(List<T> data, Expression<Func<T, string>>[] textExpressions, Expression<Func<T, string>> callbackExpression, int start, int count)
        {
            List<Func<T, string>> textFuncs = new();
            foreach(var exp in textExpressions)
                textFuncs.Add(exp.Compile());
            var callbackFunc = callbackExpression.Compile();

            var length = data.Count - start;
            if (length > count)
                length = count;

            var keyboard = new List<InlineKeyboardButton[]>();

            for (int i = start; i < start + length; i++)
            {
                var text = new StringBuilder();
                foreach (var func in textFuncs)
                    text.Append(func(data[i]));
                string callback = callbackFunc(data[i]);
                keyboard.Add([InlineKeyboardButton.WithCallbackData(text: text.ToString(), callbackData: callback)]);
            }
            return keyboard;
        }

        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase<T>(List<T> data, Expression<Func<T, string>>[] textExpression, Expression<Func<T, string>> callbackExpression)
        {
            // Вызов предыдущей функции с значениями start и count по умолчанию
            return GenerateInlineFromDatabase(data, textExpression, callbackExpression, 0, data.Count);
        }

        /// <summary>
        /// Генерирует список массивов InlineKeyboardButton для страниц с поддержкой кнопок "назад" и "вперед".
        /// </summary>
        /// <param name="page">Текущая страница.</param>
        /// <param name="callback">Префикс для обратного вызова кнопок.</param>
        /// <param name="back">Поддержка кнопки "назад".</param>
        /// <param name="front">Поддержка кнопки "вперед".</param>
        /// <returns>Список массивов кнопок для использования в InlineKeyboardMarkup.</returns>
        public static List<InlineKeyboardButton[]> InlinePages(int page, string callback, bool back, bool front)
        {
            // Преобразование номера страницы в строку с использованием эмодзи для цифр
            string pageNumber = IntegerToEmoji(page + 1);

            // Генерация клавиатуры в зависимости от поддержки кнопок "назад" и "вперед"
            if (front && back)
            {
                return new List<InlineKeyboardButton[]>()
        {
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData(text: "⬅️", callbackData: $"{callback}|{page - 1}"),
                InlineKeyboardButton.WithCallbackData(text: $"{pageNumber}", callbackData: $"{callback}|{page}"),
                InlineKeyboardButton.WithCallbackData(text: "➡️", callbackData: $"{callback}|{page + 1}")
            }
        };
            }
            if (front)
            {
                return new List<InlineKeyboardButton[]>()
        {
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData(text: $"{pageNumber}", callbackData: $"{callback}|{page}"),
                InlineKeyboardButton.WithCallbackData(text: "➡️", callbackData: $"{callback}|{page + 1}")
            }
        };
            }
            if (back)
            {
                return new List<InlineKeyboardButton[]>()
        {
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData(text: "⬅️", callbackData: $"{callback}|{page - 1}"),
                InlineKeyboardButton.WithCallbackData(text: $"{pageNumber}", callbackData: $"{callback}|{page}")
            }
        };
            }

            // Возвращение null, если не предусмотрены кнопки "назад" и "вперед"
            return null;
        }

        /// <summary>
        /// Преобразует целое число в строку, заменяя каждую цифру на соответствующее эмодзи.
        /// </summary>
        /// <param name="number">Целое число для преобразования.</param>
        /// <returns>Строка, представляющая число с использованием эмодзи.</returns>
        public static string IntegerToEmoji(int number)
        {
            Dictionary<char, string> digitToEmoji = new Dictionary<char, string>
            {
                {'0', "0️⃣"},
                {'1', "1️⃣"},
                {'2', "2️⃣"},
                {'3', "3️⃣"},
                {'4', "4️⃣"},
                {'5', "5️⃣"},
                {'6', "6️⃣"},
                {'7', "7️⃣"},
                {'8', "8️⃣"},
                {'9', "9️⃣"}
            };

            string res = "";
            foreach (var digit in number.ToString())
            {
                if (digitToEmoji.TryGetValue(digit, out var emoji))
                    res += emoji;
            }
            return res;
        }

        //For Quests
        public static InlineKeyboardMarkup FinishQuestions(string quest, int id)
        {
            return new(new InlineKeyboardButton[][]
            {
            [InlineKeyboardButton.WithCallbackData("Подтвердить", $"qconf|{quest}|{id}"),
                InlineKeyboardButton.WithCallbackData("Изменить параметр", $"qedit|{quest}|{id}")],
            });
        }
        public static InlineKeyboardMarkup EditQuestions(string quest, int id)
        {
            return new(new InlineKeyboardButton[][]
            {[InlineKeyboardButton.WithCallbackData("Изменить параметр", $"qedit|{quest}|{id}")]});
        }
        //Examples
        public static ReplyKeyboardMarkup MainMenu()
        {
            return new(GenerateMenu(TelegramBot.messageContainer.menues["main"], 2)) { ResizeKeyboard = true, IsPersistent = true };
        }
        public static InlineKeyboardMarkup WorkPlace()
        {
            return new(GenerateInline(TelegramBot.messageContainer.inlines["work_place"]));
        }
        //public static InlineKeyboardMarkup MyConstructs(Users user, int page)
        //{
        //    int count = 5;
        //    List<Constructs> constructs = DB.Select<Constructs>(s => s.tg_id == user.tg_id && s.status == true);
        //    var keyboard = GenerateInlineFromDatabase<Constructs>(constructs, [s => $"Заявка {s.id}"], s => $"construct|{s.id}", page * count, count);
        //    var pageKeyboard = InlinePages(page, "constructPages", page > 0, page + 1 < Math.Ceiling((double)constructs.Count / count));
        //    if (pageKeyboard != null)
        //        keyboard.AddRange(pageKeyboard);
        //    return new(keyboard);
        //}
    }
}
