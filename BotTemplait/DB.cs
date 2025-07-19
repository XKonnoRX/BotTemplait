using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Mysqlx.Notice;
using BotTemplait;

public class DB : DbContext
{
    public DbSet<BotTemplait.UserData> Users { get; set; }
    public static string connectionString { get; set; }

    private ValueConverter<T, string> Serializer<T>()
    {
        return new ValueConverter<T, string>(
         v => JsonConvert.SerializeObject(v),  // сериализация в строку JSON
         v => JsonConvert.DeserializeObject<T>(v) // десериализация из строки JSON
     );
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(connectionString,
            new MySqlServerVersion(new Version(8, 0, 21)))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<UserData>()
        //     .Property(e => e.json)
        //     .HasConversion(Serializer<Json>())
        //     .HasColumnType("JSON");
    }
    public static void CreateTables()
    {
        using var context = new DB();
        context.Database.EnsureCreated();
    }
    public static T Insert<T>(T entity) where T : class
    {
        using var context = new DB();
        context.Set<T>().Add(entity);
        context.SaveChanges();
        context.Entry(entity).Reload();
        return entity;
    }
    public static T Insert<T>(T entity, Func<T?, bool> returnValue) where T : class
    {
        using var context = new DB();
        context.Set<T>().Add(entity);
        context.SaveChanges();

        return context.Set<T>().FirstOrDefault(returnValue, null);

    }
    public static T Find<T>(Func<T?, bool> condition) where T : class
    {
        using var context = new DB();
        return context.Set<T>().FirstOrDefault(condition, null);
    }
    public static bool Delete<T>(Func<T?, bool> condition) where T : class
    {
        using var context = new DB();
        T content = context.Set<T>().FirstOrDefault(condition, null);
        if (content != null)
        {
            context.Remove(content);
            context.SaveChanges();
            return true;
        }
        return false;
    }
    public static bool DeleteAll<T>(Func<T, bool> condition) where T : class
    {
        using var context = new DB();
        var itemsToDelete = context.Set<T>().Where(condition).ToList();
        if (itemsToDelete.Any())
        {
            context.RemoveRange(itemsToDelete);
            context.SaveChanges();
            return true;
        }
        return false;
    }
    public static bool Delete<T>(T content) where T : class
    {
        using var context = new DB();
        if (content != null)
        {
            context.Remove(content);
            context.SaveChanges();
            return true;
        }
        return false;
    }
    public static T GetMax<T, TKey>(Expression<Func<T, TKey>> condition) where T : class
    {
        using var context = new DB();
        return context.Set<T>().OrderByDescending(condition).FirstOrDefault();
    }
    public static T GetMax<T, TKey>(Func<T, bool> condition, Expression<Func<T, TKey>> index) where T : class
    {
        using var context = new DB();
        return context.Set<T>().OrderByDescending(index).Where(condition).FirstOrDefault();
    }
    public static List<T> Select<T>(Func<T, bool> condition) where T : class
    {
        using var context = new DB();
        return [.. context.Set<T>().Where(condition)];
    }
    public static List<T> Select<T>() where T : class
    {
        using var context = new DB();
        return [.. context.Set<T>()];
    }
    public static void Update<T>(Func<T, bool> condition, Action<T> updateAction) where T : class
    {
        using var context = new DB();
        var entity = context.Set<T>().FirstOrDefault(condition, null);
        if (entity != null)
        {
            updateAction(entity);
            context.SaveChanges();
        }
    }
    public static void UpdateEntity<T>(T target, T source) where T : class
    {
        using var context = new DB();
        context.Set<T>().Attach(target);
        var properties = typeof(T).GetProperties()
            .Where(prop => prop.CanRead && prop.CanWrite);
        foreach (var prop in properties)
        {
            var value = prop.GetValue(source, null);
            prop.SetValue(target, value, null);
        }
        context.SaveChanges();
    }
    public static void Send(string command)
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
}