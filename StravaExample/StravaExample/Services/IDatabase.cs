using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StravaExample.Services
{
    public interface IDatabase
    {
        Task Initialize();
        Task<T> Get<T>(object pk) where T : new();
        Task<List<T>> GetAll<T>() where T : new();
        Task<T> GetFirst<T>() where T : new();
        Task<int> Insert<T>(T t);
        Task<int> InsertAll<T>(IEnumerable<T> t);
        Task<int> Update<T>(T t);
        Task<int> Delete<T>(T t) where T : class;
        Task<int> Delete<T>(object primaryKey) where T : class;
        Task<int> Execute(string query, params object[] args);
        Task<List<T>> Query<T>(string query, params object[] args) where T : new();
    }
}
