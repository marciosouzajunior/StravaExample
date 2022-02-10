using SQLite;
using StravaExample.Models;
using StravaExample.Services.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(Database))]
namespace StravaExample.Services.Impl
{
    public class Database : IDatabase
    {
        string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "myapp.db3");
        SQLiteAsyncConnection connection => new SQLiteAsyncConnection(databasePath);
        public async Task Initialize()
        {
            try
            {
                await connection.ExecuteAsync("CREATE TABLE IF NOT EXISTS DatabaseVersion (version INTEGER)");
                int databaseVersion = await connection.ExecuteScalarAsync<int>("SELECT IFNULL(version, 0) FROM DatabaseVersion");

                if (databaseVersion < 1)
                {
                    await connection.CreateTableAsync<AppActivity>();
                    await connection.CreateTableAsync<AppActivityHR>();
                    await connection.CreateTableAsync<StravaSync>();
                    await connection.ExecuteAsync("INSERT INTO DatabaseVersion VALUES (1)");
                }

                if (databaseVersion < 2)
                {
                    //await connection.ExecuteAsync("");
                    //await connection.ExecuteAsync("UPDATE DatabaseVersion SET numero = 2;");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error creating database: " + e.Message);
            }

        }

        public Task<T> Get<T>(object pk) where T : new()
        {
            return connection.GetAsync<T>(pk);
        }

        public Task<List<T>> GetAll<T>() where T : new()
        {
            return connection.Table<T>().ToListAsync();
        }

        public async Task<T> GetFirst<T>() where T : new()
        {
            List<T> list = await connection.Table<T>().ToListAsync();
            if (list.Count == 0)
                throw new InvalidOperationException();
            return list[0];
        }

        public Task<int> Insert<T>(T t)
        {
            return connection.InsertAsync(t);
        }

        public Task<int> InsertAll<T>(IEnumerable<T> t)
        {
            return connection.InsertAllAsync(t, true);
        }

        public Task<int> Update<T>(T t)
        {
            return connection.UpdateAsync(t);
        }

        public Task<int> Delete<T>(T t) where T : class
        {
            return connection.DeleteAsync(t);
        }

        public Task<int> Delete<T>(object primaryKey) where T : class
        {
            return connection.DeleteAsync<T>(primaryKey);
        }

        public Task<int> Execute(string query, params object[] args)
        {
            return connection.ExecuteAsync(query, args);
        }

        public Task<List<T>> Query<T>(string query, params object[] args) where T : new()
        {
            return connection.QueryAsync<T>(query, args);
        }
    }
}
