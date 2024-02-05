using MySql.Data.MySqlClient;

namespace BotTemplait
{
    
    internal class DataBase
    {
        public static string connectionString { get; set; }
        public static string Schema { get; set; }
        /// <summary>
        /// Выполняет переданную SQL-команду в базе данных MySQL, используя подключение, предоставленное в строке подключения.
        /// </summary>
        /// <param name="command">SQL-команда для выполнения в базе данных.</param>
        /// <remarks>
        /// Этот метод автоматически устанавливает и освобождает соединение с базой данных, а также выполняет переданную команду
        /// без возвращения результата. В случае необходимости выполнения команд с возвращением результатов, следует использовать
        /// другие методы, предоставляющие соответствующую функциональность.
        /// </remarks>
        public static void Send(string command)
        {
            // Использование конструкции using для автоматического освобождения ресурсов после использования
            using (var conn = new MySqlConnection(connectionString))
            {
                // Открытие соединения с базой данных
                conn.Open();

                // Использование конструкции using для создания команды MySqlCommand с переданной командой и соединением
                using (var cmd = new MySqlCommand(command, conn))
                {
                    // Выполнение команды без возврата результата
                    cmd.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// Выполняет SQL-команду и возвращает значение первой строки и первого столбца результата запроса в виде целочисленного значения.
        /// </summary>
        /// <param name="command">SQL-команда для выполнения в базе данных.</param>
        /// <returns>Целочисленное значение, представляющее результат выполнения команды.</returns>
        /// <remarks>
        /// Если запрос возвращает множество строк и столбцов, метод возвращает значение из первой строки и первого столбца.
        /// В случае ошибки или отсутствия данных возвращает 0.
        /// </remarks>
        public static int ExecuteScalar(string command)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(command, conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Выполняет SQL-команду и возвращает первую строку результата запроса в виде массива объектов.
        /// </summary>
        /// <param name="command">SQL-команда для выполнения в базе данных.</param>
        /// <returns>Массив объектов, представляющих значения первой строки результата запроса.</returns>
        /// <remarks>
        /// Если запрос не возвращает данные, метод возвращает null.
        /// </remarks>
        public static object[] Read(string command)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(command, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var myObject = new object[reader.FieldCount];
                            reader.GetValues(myObject);
                            return myObject;
                        }
                        else
                            return null;
                    }
                }
            }
        }

        /// <summary>
        /// Выполняет SQL-команду и возвращает результат запроса в виде массива массивов объектов.
        /// Каждый массив объектов представляет значения одной строки результата.
        /// </summary>
        /// <param name="data">SQL-команда для выполнения в базе данных.</param>
        /// <returns>Массив массивов объектов, представляющих результат запроса.</returns>
        /// <remarks>
        /// Если запрос не возвращает данные, метод возвращает null.
        /// </remarks>
        public static object[][] ReadMultiline(string data)
        {
            using (MySqlConnection sqlConnection = new MySqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (MySqlCommand command = new MySqlCommand(data, sqlConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        try
                        {
                            var lines = new List<object[]>();
                            while (reader.Read())
                            {
                                var myObject = new object[reader.FieldCount];
                                reader.GetValues(myObject);
                                lines.Add(myObject);
                            }
                            return lines.ToArray();
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Добавляет запись в лог-файл, содержащую переданный контент с датой и временем.
        /// </summary>
        /// <param name="path">Путь к лог-файлу, в который будет добавлена запись.</param>
        /// <param name="content">Контент, который будет добавлен в лог-файл.</param>
        /// <returns>
        /// Возвращает true, если запись успешно добавлена в лог-файл, иначе возвращает false.
        /// </returns>
        /// <remarks>
        /// Метод формирует строку, содержащую дату и время в формате "yyyy-MM-dd HH:mm:ss", а также переданный контент.
        /// Затем строка выводится в консоль и добавляется в указанный лог-файл.
        /// В случае успешного выполнения возвращает true, в случае ошибки - false.
        /// </remarks>
        public static bool Log(string path, string content)
        {
            try
            {
                // Получение текущей даты и времени
                var date = DateTime.Now;

                // Формирование строки для записи в лог: "гггг-ММ-дд ЧЧ:мм:сс контент"
                string logEntry = $"{date.ToString("yyyy-MM-dd HH:mm:ss")} {content}";

                // Вывод строки в консоль
                Console.WriteLine(logEntry);

                // Добавление строки в лог-файл
                File.AppendAllText(path, $"{logEntry}\n");

                // Возвращение успешного результата
                return true;
            }
            catch
            {
                // Возвращение ошибочного результата в случае исключения
                return false;
            }
        }

    }
}
