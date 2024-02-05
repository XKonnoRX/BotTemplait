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
        /// Получает массив кнопок для меню с указанным именем из контейнера сообщений телеграм-бота.
        /// </summary>
        /// <param name="name">Имя меню.</param>
        /// <returns>Массив строк, представляющих кнопки меню, или null, если меню не найдено.</returns>
        public static string[] GetMenuButtons(string name)
        {
            var menues = TelegramBot.messageContainer.menues;

            // Поиск меню с указанным именем
            foreach (var menu in menues)
            {
                if (menu.name == name)
                    return menu.buttons;
            }

            // Возвращаем null, если меню не найдено
            return null;
        }
        /// <summary>
        /// Получает массив кнопок для встроенного (inline) меню с указанным именем из контейнера сообщений телеграм-бота.
        /// </summary>
        /// <param name="name">Имя встроенного меню.</param>
        /// <returns>
        /// Массив кнопок встроенного меню или null, если встроенное меню с указанным именем не найдено.
        /// </returns>
        public static Button[] GetInlineButtons(string name)
        {
            // Получаем все встроенные меню из контейнера сообщений телеграм-бота
            var inlines = TelegramBot.messageContainer.inlines;

            // Поиск встроенного меню с указанным именем
            foreach (var inline in inlines)
            {
                if (inline.name == name)
                    return inline.buttons;
            }

            // Возвращаем null, если встроенное меню с указанным именем не найдено
            return null;
        }
        /// <summary>
        /// Генерирует встроенные кнопки клавиатуры для телеграм-бота на основе переданных параметров.
        /// </summary>
        /// <param name="buttons">Массив объектов кнопок.</param>
        /// <param name="callback">Callback-данные, общие для всех кнопок.</param>
        /// <param name="start">Начальный индекс кнопки в массиве.</param>
        /// <param name="count">Количество кнопок, которые следует включить в результат.</param>
        /// <returns>Список массивов встроенных кнопок для телеграм-бота.</returns>
        public static List<InlineKeyboardButton[]> GenerateInline(Button[] buttons, string callback, int start, int count)
        {
            var length = buttons.Length - start;

            // Определяем количество кнопок, которые будут включены в результат
            if (length > count)
                length = count;

            var keyboard = new List<InlineKeyboardButton[]>();

            // Генерируем встроенные кнопки для каждой кнопки в диапазоне
            for (int i = start; i < start + length; i++)
            {
                switch (buttons[i].type)
                {
                    case "text":
                        // Добавляем встроенную кнопку с текстовыми callback-данными
                        keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: buttons[i].text, callbackData: $"{buttons[i].back}|{callback}") });
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
        /// <param name="callback">Callback-данные, общие для всех кнопок.</param>
        /// <returns>Список массивов встроенных кнопок для телеграм-бота.</returns>
        public static List<InlineKeyboardButton[]> GenerateInline(Button[] buttons, string callback)
        {
            // Вызываем первую версию метода с начальным индексом 0 и количеством кнопок равным длине массива
            return GenerateInline(buttons, callback, 0, buttons.Length);
        }

        /// <summary>
        /// Генерирует встроенные кнопки клавиатуры для телеграм-бота на основе переданных параметров.
        /// </summary>
        /// <param name="buttons">Массив объектов кнопок.</param>
        /// <returns>Список массивов встроенных кнопок для телеграм-бота.</returns>
        public static List<InlineKeyboardButton[]> GenerateInline(Button[] buttons)
        {
            // Вызываем первую версию метода с начальным индексом 0, количеством кнопок равным длине массива, и пустым callback
            return GenerateInline(buttons, "", 0, buttons.Length);
        }
        /// <summary>
        /// Генерирует список массивов InlineKeyboardButton на основе данных из базы.
        /// </summary>
        /// <param name="buttons">Двумерный массив объектов, представляющих кнопки.</param>
        /// <param name="text">Индекс столбца с текстом кнопки.</param>
        /// <param name="callback">Индекс столбца с обратным вызовом (callback) кнопки.</param>
        /// <param name="active">Индекс столбца, указывающего активность кнопки (1 - активна, 0 - неактивна).</param>
        /// <param name="start">Индекс, с которого начинать выборку кнопок.</param>
        /// <param name="count">Максимальное количество кнопок для выборки.</param>
        /// <returns>Список массивов кнопок для использования в InlineKeyboardMarkup.</returns>
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int text, int callback, int active, int start, int count)
        {
            // Вычисление effective length на основе параметров start и count
            var length = buttons.Length - start;
            if (length > count)
                length = count;

            // Инициализация списка для хранения массивов InlineKeyboardButton
            var keyboard = new List<InlineKeyboardButton[]>();

            // Перебор указанного диапазона и добавление кнопок в клавиатуру, если active равен 1
            for (int i = start; i < start + length; i++)
                if (buttons[i][active].ToString() == "1")
                    keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: buttons[i][text].ToString(), callbackData: buttons[i][callback].ToString()) });

            return keyboard;
        }

        /// <summary>
        /// Генерирует список массивов InlineKeyboardButton на основе данных из базы с использованием значений по умолчанию для start и count.
        /// </summary>
        /// <param name="buttons">Двумерный массив объектов, представляющих кнопки.</param>
        /// <param name="text">Индекс столбца с текстом кнопки.</param>
        /// <param name="callback">Индекс столбца с обратным вызовом (callback) кнопки.</param>
        /// <param name="active">Индекс столбца, указывающего активность кнопки (1 - активна, 0 - неактивна).</param>
        /// <returns>Список массивов кнопок для использования в InlineKeyboardMarkup.</returns>
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int text, int callback, int active)
        {
            // Вызов предыдущей функции с значениями start и count по умолчанию
            return GenerateInlineFromDatabase(buttons, text, callback, active, 0, buttons.Length);
        }

        /// <summary>
        /// Генерирует список массивов InlineKeyboardButton на основе данных из базы.
        /// </summary>
        /// <param name="buttons">Двумерный массив объектов, представляющих кнопки.</param>
        /// <param name="text">Индекс столбца с текстом кнопки.</param>
        /// <param name="callback">Индекс столбца с обратным вызовом (callback) кнопки.</param>
        /// <param name="start">Индекс, с которого начинать выборку кнопок.</param>
        /// <param name="count">Максимальное количество кнопок для выборки.</param>
        /// <returns>Список массивов кнопок для использования в InlineKeyboardMarkup.</returns>
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int text, int callback, int start, int count)
        {
            // Similar to the first function, but without the active check
            var length = buttons.Length - start;
            if (length > count)
                length = count;

            var keyboard = new List<InlineKeyboardButton[]>();

            // Iterate through the specified range and add buttons to the keyboard
            for (int i = start; i < start + length; i++)
                keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: buttons[i][text].ToString(), callbackData: buttons[i][callback].ToString()) });

            return keyboard;
        }

        /// <summary>
        /// Генерирует список массивов InlineKeyboardButton на основе данных из базы.
        /// </summary>
        /// <param name="buttons">Двумерный массив объектов, представляющих кнопки.</param>
        /// <param name="text">Индекс столбца с текстом кнопки.</param>
        /// <param name="callback">Индекс столбца с обратным вызовом (callback) кнопки.</param>
        /// <returns>Список массивов кнопок для использования в InlineKeyboardMarkup.</returns>
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int text, int callback)
        {
            // Call the function with default start and count values
            return GenerateInlineFromDatabase(buttons, text, callback, 0, buttons.Length);
        }

        /// <summary>
        /// Генерирует список массивов InlineKeyboardButton на основе данных из базы.
        /// </summary>
        /// <param name="buttons">Двумерный массив объектов, представляющих кнопки.</param>
        /// <param name="text">Массив индексов столбцов с текстом кнопки.</param>
        /// <param name="precallback">Префикс для обратного вызова кнопки.</param>
        /// <param name="callback">Индекс столбца с обратным вызовом (callback) кнопки.</param>
        /// <param name="start">Индекс, с которого начинать выборку кнопок.</param>
        /// <param name="count">Максимальное количество кнопок для выборки.</param>
        /// <returns>Список массивов кнопок для использования в InlineKeyboardMarkup.</returns>
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int[] text, string precallback, int callback, int start, int count)
        {
            var length = buttons.Length - start;
            if (length > count)
                length = count;

            var keyboard = new List<InlineKeyboardButton[]>();

            // Iterate through the specified range and concatenate text values
            for (int i = start; i < start + length; i++)
            {
                string fulltext = "";
                foreach (var t in text)
                    fulltext += buttons[i][t].ToString() + " ";

                // Add buttons to the keyboard with concatenated text and callback data
                keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: fulltext, callbackData: $"{precallback}|{buttons[i][callback]}") });
            }

            return keyboard;
        }
        /// <summary>
        /// Генерирует список массивов InlineKeyboardButton на основе данных из базы, используя значения по умолчанию для start и count.
        /// </summary>
        /// <param name="buttons">Двумерный массив объектов, представляющих кнопки.</param>
        /// <param name="text">Массив индексов столбцов с текстом кнопки.</param>
        /// <param name="precallback">Префикс для обратного вызова кнопки.</param>
        /// <param name="callback">Индекс столбца с обратным вызовом (callback) кнопки.</param>
        /// <returns>Список массивов кнопок для использования в InlineKeyboardMarkup.</returns>
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int[] text, string precallback, int callback)
        {
            // Вызов предыдущей функции с значениями start и count по умолчанию
            return GenerateInlineFromDatabase(buttons, text, precallback, callback, 0, buttons.Length);
        }

        /// <summary>
        /// Генерирует список массивов InlineKeyboardButton на основе данных из базы с использованием дополнительных параметров.
        /// </summary>
        /// <param name="buttons">Двумерный массив объектов, представляющих кнопки.</param>
        /// <param name="name">Индекс столбца с именем кнопки.</param>
        /// <param name="text">Индекс столбца с текстом кнопки.</param>
        /// <param name="textlenght">Длина текста, которая будет отображена в кнопке.</param>
        /// <param name="precallback">Префикс для обратного вызова кнопки.</param>
        /// <param name="callback">Индекс столбца с обратным вызовом (callback) кнопки.</param>
        /// <param name="start">Индекс, с которого начинать выборку кнопок.</param>
        /// <param name="count">Максимальное количество кнопок для выборки.</param>
        /// <returns>Список массивов кнопок для использования в InlineKeyboardMarkup.</returns>
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int name, int text, int textlenght, string precallback, int callback, int start, int count)
        {
            var length = buttons.Length - start;
            if (length > count)
                length = count;

            var keyboard = new List<InlineKeyboardButton[]>();

            // Итерация по указанному диапазону и создание кнопок с заданными параметрами
            for (int i = start; i < start + length; i++)
            {
                string fulltext = "";

                // Проверка, не пуст ли текст кнопки
                if (buttons[i][text].ToString() != "")
                {
                    int textLength = textlenght;

                    // Урезание текста, если его длина превышает заданную длину
                    if (textLength > buttons[i][text].ToString().Length)
                        textLength = buttons[i][text].ToString().Length;

                    // Формирование полного текста для кнопки
                    fulltext = $"{buttons[i][name]} ({buttons[i][text].ToString().Substring(0, textLength)}...)";
                }
                else
                {
                    // Если текст кнопки пуст, используется только имя
                    fulltext = $"{buttons[i][name]}";
                }

                // Добавление кнопок в клавиатуру с обратным вызовом
                keyboard.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: fulltext, callbackData: $"{precallback}|{buttons[i][callback]}") });
            }

            return keyboard;
        }

        /// <summary>
        /// Генерирует список массивов InlineKeyboardButton на основе данных из базы с использованием значений по умолчанию для start и count.
        /// </summary>
        /// <param name="buttons">Двумерный массив объектов, представляющих кнопки.</param>
        /// <param name="name">Индекс столбца с именем кнопки.</param>
        /// <param name="text">Индекс столбца с текстом кнопки.</param>
        /// <param name="textlenght">Длина текста, которая будет отображена в кнопке.</param>
        /// <param name="precallback">Префикс для обратного вызова кнопки.</param>
        /// <param name="callback">Индекс столбца с обратным вызовом (callback) кнопки.</param>
        /// <returns>Список массивов кнопок для использования в InlineKeyboardMarkup.</returns>
        public static List<InlineKeyboardButton[]> GenerateInlineFromDatabase(object[][] buttons, int name, int text, int textlenght, string precallback, int callback)
        {
            // Вызов функции с использованием значений по умолчанию для start и count
            return GenerateInlineFromDatabase(buttons, name, text, textlenght, precallback, callback, 0, buttons.Length);
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
                {
                    res += emoji;
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
            var reader = DataBase.ReadMultiline("SELECT * FROM cars WHERE Status = 'Active'");
            return new(GenerateInlineFromDatabase(reader, new int[] { 1, 0 }, "car", 0));
        }
        public static InlineKeyboardMarkup WorkPlace()
        {
            return new(GenerateInline(GetInlineButtons("work_place")));
        }
    }
}
