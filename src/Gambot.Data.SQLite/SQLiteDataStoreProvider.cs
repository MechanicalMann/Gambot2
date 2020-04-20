using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Gambot.Data.SQLite
{
    public class SQLiteDataStoreProvider : IDataStoreProvider
    {
        private readonly IDbConnection _connection;

        private readonly Dictionary<string, IDataStore> _dataStores = new Dictionary<string, IDataStore>(StringComparer.OrdinalIgnoreCase);

        public SQLiteDataStoreProvider(string connectionString)
        {
            _connection = new SQLiteConnection(connectionString);
            _connection.Open();
        }

        public async Task<IDataStore> GetDataStore(string key)
        {
            if (_dataStores.TryGetValue(key, out var dataStore))
                return dataStore;
            
            var ds = new SQLiteDataStore(_connection, key.ToLowerInvariant());
            await ds.Initialize();
            _dataStores.Add(key, ds);
            return ds;
        }
    }
}