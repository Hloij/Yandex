using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.WorkerClass
{
    public class WorkWithBD<T> where T : new()
    {
        static SqliteConnection connection;
        public static bool wascon;
        static async Task Conect()
        {
            try
            {
                SQLitePCL.Batteries.Init();
                string path = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
                path += @"\YandexBD.db";
                connection = new SqliteConnection($"Data Source={path}");

                await connection.OpenAsync();
                wascon = true;
            }
            catch (Exception ex)
            {
                wascon = false;
            }
        }
        public static async Task Change(T obj)
        {
            await Conect();
            Type type = obj.GetType();
            string sqlExpression = $"UPDATE '{type.Name}'SET ";
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                if (i + 1 == properties.Length)
                {
                    sqlExpression += $"'{properties[i].Name}'={properties[i].GetValue(obj)}";
                }
                else
                {
                    sqlExpression += $"'{properties[i].Name}'={properties[i].GetValue(obj)},";
                }

            }
            sqlExpression += $"WHERE `{properties[0].Name}` = '{properties[0].GetValue(obj)}';";
            SqliteCommand command = new SqliteCommand(sqlExpression, connection);
            await command.ExecuteNonQueryAsync();
            command.Cancel();
            connection.Close();
        }
        public static async Task Del(T obj)
        {
            await Conect();
            Type type = obj.GetType();
            string sqlExpression = $"DELETE FROM '{type.Name}' ";
            PropertyInfo[] properties = type.GetProperties();
            sqlExpression += $"WHERE `{properties[0].Name}` = '{properties[0].GetValue(obj)}';";
            SqliteCommand command = new SqliteCommand(sqlExpression, connection);
            await command.ExecuteNonQueryAsync();
            command.Cancel();
            connection.Close();
        }
        public static async Task Add(T obj)
        {
            await Conect();
            Type type = obj.GetType();
            string sqlExpression = $"INSERT INTO '{type.Name}' (";
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                if (i+1 == properties.Length)
                {
                    sqlExpression += $"'{properties[i].Name}')";
                }
                else
                {
                    sqlExpression += $"'{properties[i].Name}',";
                }

            }
            sqlExpression += $"VALUES (";
            for (int i = 0; i < properties.Length; i++)
            {
                if (i + 1 == properties.Length)
                {
                    sqlExpression += $"'{properties[i].GetValue(obj)}')";
                }
                else
                {
                    sqlExpression += $"'{properties[i].GetValue(obj)}',";
                }

            }
            SqliteCommand command = new SqliteCommand(sqlExpression, connection);
            await command.ExecuteNonQueryAsync();
            command.Cancel();
            connection.Close();
        }
        public static async Task<List<T>> Read(T obj) 
        {
            Conect().Wait();
            List<T> values = new List<T>();
            Type type = obj.GetType();
            string sqlExpression = $"SELECT * FROM '{type.Name}' ";
            SqliteCommand command = new SqliteCommand(sqlExpression, connection);
            PropertyInfo[] properties = type.GetProperties();
            
            using (SqliteDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows) // если есть данные
                { 
                    while (reader.Read())   // построчно считываем данные
                    {
                        T value = new T();
                        for (int i = 0; i < properties.Length; i++)
                        {
                           
                            var vs = reader.GetValue(i);
                            try
                            {
                                properties[i].SetValue(value, vs);
                            }
                            catch
                            {
                                if (properties[i].PropertyType.Name == "Double")
                                {
                                    properties[i].SetValue(value, Convert.ToDouble(vs));
                                }
                                if (properties[i].PropertyType.Name == "DateTime")
                                {
                                    properties[i].SetValue(value, Convert.ToDateTime(vs));
                                }
                                
                                
                            }
                        }

                        values.Add(value);
                    }
                }
            }
            command.Cancel();
            return values;
        }
        static T CreateObject<T>() where T : new()
        {
            return new T(); // Используется оператор new()
        }
    }
}
