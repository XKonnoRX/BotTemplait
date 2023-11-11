using MySql.Data.MySqlClient;

namespace BotTemplait
{
    
    internal class DataBase
    {
        public static string connectionString { get; set; }
        public static void SendCommand(string command)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(command, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
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
        public static object[][] ReaderMultiline(string data)
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
        public static bool Log(string path, string content)
        {
            try
            {
                var date = DateTime.Now;
                string stroke = $"{date.ToString("yyyy-MM-dd HH:mm:ss")} {content}";
                Console.WriteLine(stroke);
                File.AppendAllText(path, $"{content}\n");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
