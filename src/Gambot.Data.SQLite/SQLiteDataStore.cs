using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Gambot.Data;

namespace Gambot.Data.SQLite
{
    public class SQLiteDataStore : IDataStore
    {
        private readonly IDbConnection _connection;
        private readonly string _dataStore;

        public SQLiteDataStore(IDbConnection connection, string dataStore)
        {
            _connection = connection;
            _dataStore = dataStore;
        }

        private Task<int> Execute(string query, object args = null)
        {
            // So it turns out SQLite *still* doesn't support async IO :|
            // I won't refactor the entire codebase to be synchronous just yet
            return Task.Run(() =>
            {
                using(var cmd = CreateCommand(query, args))
                {
                    return cmd.ExecuteNonQuery();
                }
            });
        }

        private Task<IEnumerable<DataStoreValue>> Query(string query, object args = null)
        {
            return Task.Run<IEnumerable<DataStoreValue>>(() =>
            {
                var data = new List<DataStoreValue>();
                using(var cmd = CreateCommand(query, args))
                using(var reader = cmd.ExecuteReader())
                while (reader.Read())
                {
                    var row = new DataStoreValue();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var column = reader.GetName(i);
                        if (column == "rowid")
                            row.Id = reader.GetInt32(i);
                        else if (column == "key")
                            row.Key = reader.GetString(i);
                        else if (column == "value")
                            row.Value = reader.GetString(i);
                    }
                    data.Add(row);
                }
                return data;
            });
        }

        private IDbCommand CreateCommand(string query, object args = null)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = query;
            if (args == null)
                return cmd;

            foreach (var a in args.GetType().GetProperties())
                cmd.Parameters.Add(new SQLiteParameter($"@{a.Name}", a.GetValue(args)));

            return cmd;
        }

        internal async Task Initialize()
        {
            var query = $"create table if not exists \"{_dataStore}\" (\"key\" text, \"value\" text);";
            await Execute(query);
        }

        public async Task<bool> Add(string key, string value)
        {
            var query = $"insert into \"{_dataStore}\"(\"key\", \"value\") values(@key, @value);";
            var args = new { key, value };
            return await Execute(query, args) > 0;
        }

        public async Task<DataStoreValue> Get(int id)
        {
            var query = $"select rowid, key, value from \"{_dataStore}\" where id = @id;";
            var args = new { id };
            return (await Query(query, args)).FirstOrDefault();
        }

        public async Task<IEnumerable<DataStoreValue>> GetAll(string key)
        {
            var query = $"select rowid, key, value from \"{_dataStore}\" where key like @key;";
            var args = new { key };
            return await Query(query, args);
        }

        public async Task<IEnumerable<string>> GetAllKeys()
        {
            var query = $"select distinct key from \"{_dataStore}\";";
            return (await Query(query)).Select(x => x.Key);
        }

        public async Task<DataStoreValue> GetRandom(string key)
        {
            var query = $"select rowid, key, value from \"{_dataStore}\" where key like @key order by random() limit 1;";
            var args = new { key };
            return (await Query(query, args)).FirstOrDefault();
        }

        public async Task<DataStoreValue> GetRandom()
        {
            var query = $"select rowid, key, value from \"{_dataStore}\" order by random() limit 1;";
            return (await Query(query)).FirstOrDefault();
        }

        public async Task<bool> Remove(string key, string value)
        {
            var query = $"delete from {_dataStore} where \"key\" like @key and \"value\" like @value;";
            var args = new { key, value };
            return await Execute(query, args) > 0;
        }

        public async Task<bool> Remove(int id)
        {
            var query = $"DELETE FROM \"{_dataStore}\" WHERE \"rowid\" = @id;";
            var args = new { id };
            return await Execute(query, args) > 0;
        }

        public async Task<int> RemoveAll(string key)
        {
            var query = $"delete from \"{_dataStore}\" where \"key\" like @key;";
            var args = new { key };
            return await Execute(query, args);
        }
    }
}