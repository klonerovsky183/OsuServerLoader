using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace OsuServerLoader.Services
{
    public class Server
    {
        public string label { get; set; }
        public string devflag { get; set; }
        public string nickname { get; set; }
        public string password { get; set; }
    }

    internal class DataService
    {
        public void CreateDataFile()
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathDataFile = System.IO.Path.Combine(userFolderPath, ".OsuServerLoader\\ServerBase.db");

            var dbFile = File.Create(pathDataFile);
            dbFile.Close();

            using (var connection = new SqliteConnection("Data Source=" + pathDataFile))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "CREATE TABLE servers(" +
                    "label string," +
                    "devflag string," +
                    "nickname string," +
                    "password string);";
                command.ExecuteNonQuery();
            }
        }

        public void AddRow(Server account)
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathDataFile = System.IO.Path.Combine(userFolderPath, ".OsuServerLoader\\ServerBase.db");

            using (var connection = new SqliteConnection("Data Source=" + pathDataFile))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "INSERT INTO servers (label, devflag , nickname, password) VALUES (@label, @devflag, @nickname, @password);";
                command.Parameters.AddWithValue("label", account.label.Replace(" ", "_"));
                command.Parameters.AddWithValue("devflag", account.devflag);
                command.Parameters.AddWithValue("nickname", account.nickname);
                command.Parameters.AddWithValue("password", account.password);
                command.ExecuteNonQuery();
            }
        }

        public List<Server> LoadServers()
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathDataFile = System.IO.Path.Combine(userFolderPath, ".OsuServerLoader\\ServerBase.db");
            List<Server> servers = new List<Server>();
            Server server = new Server();

            using (var connection = new SqliteConnection("Data Source=" + pathDataFile))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "SELECT * FROM servers";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            server.label = reader.GetValue(0).ToString();
                            server.devflag = reader.GetValue(1).ToString();
                            server.nickname = reader.GetValue(2).ToString();
                            server.password = reader.GetValue(3).ToString();
                            servers.Add(new Server
                            {
                                label = server.label,
                                devflag = server.devflag,
                                nickname = server.nickname,
                                password = server.password
                            });
                        }
                    }
                }
            }
            return servers;
        }

        public void DeleteRow(string label)
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathDataFile = System.IO.Path.Combine(userFolderPath, ".OsuServerLoader\\ServerBase.db");

            using (var connection = new SqliteConnection("Data Source=" + pathDataFile))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "DELETE FROM servers WHERE label = @label";
                command.Parameters.AddWithValue("label", label);
                command.ExecuteNonQuery();
            }
        }

        public Server GetServer(string label)
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathDataFile = System.IO.Path.Combine(userFolderPath, ".OsuServerLoader\\ServerBase.db");
            Server server = new Server();

            using (var connection = new SqliteConnection("Data Source=" + pathDataFile))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "SELECT * FROM servers WHERE label = @label LIMIT 1";
                command.Parameters.AddWithValue("label", label);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            server.label = label;
                            server.devflag = reader.GetString(1);
                            server.nickname = reader.GetString(2);
                            server.password = reader.GetString(3);
                        }
                    }
                }
            }
            return server;
        }
    }
}