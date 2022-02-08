using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;

namespace Buildmode
{
    internal static class Data
    {
        private static IDbConnection db;

        public static void Initialize()
        {
            switch (TShock.Config.Settings.StorageType.ToLower())
            {
                case "mysql":
                    {
                        var dbHost = TShock.Config.Settings.MySqlHost.Split(':');
                        db = new MySqlConnection($"Server={dbHost[0]};" +
                                $"Port={(dbHost.Length == 1 ? "3306" : dbHost[1])};" +
                                $"Database={TShock.Config.Settings.MySqlDbName};" +
                                $"Uid={TShock.Config.Settings.MySqlUsername};" +
                                $"Pwd={TShock.Config.Settings.MySqlPassword};");
                    }
                    break;
                case "sqlite":
                    db = new SqliteConnection($"uri=file://{Path.Combine(TShock.SavePath, "buildmode.sqlite")},Version=3");
                    break;
                default:
                    throw new ArgumentException();
            }

            SqlTableCreator creator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite 
                ? (IQueryBuilder)new SqliteQueryCreator() 
                : new MysqlQueryCreator());

            creator.EnsureTableStructure(new SqlTable("buildmode",
                new SqlColumn("id", MySqlDbType.Int32) { Primary = true, Unique = true },
                new SqlColumn("buffs", MySqlDbType.Text)));
        }

        public static IEnumerable<int> Read(int id)
        {
            string query = $"SELECT * FROM buildmode WHERE id = {id}";
            using (var result = db.QueryReader(query))
            {
                if (result.Read())
                {
                    var readout = result.Get<string>("buffs");
                    return readout.Split(' ')
                        .Select(x => int.Parse(x));
                }

                else
                {
                    Create(id);
                    return Config.Settings.BaseBuffs;
                }
            }
        }

        private static void Create(int id)
            => db.Query("INSERT INTO buildmode (id, buffs) VALUES (@0, @1);", 
                id, 
                string.Join(" ", Config.Settings.BaseBuffs));

        public static void Update(IEnumerable<int> buffs, int id)
            => db.Query("UPDATE buildmode SET buffs = @0 WHERE id = @1", 
                string.Join(" ", buffs), 
                id);

        public static void Reset(int id)
            => db.Query("UPDATE buildmode SET buffs = @0 WHERE id = @1", 
                string.Join(" ", Config.Settings.BaseBuffs), 
                id);
    }
}
