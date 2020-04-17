using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Gambot.Data.InMemory
{
    public class InMemoryDataStoreProvider : IDataStoreProvider
    {
        private readonly ConcurrentDictionary<string, IDataStore> _dataStores = new ConcurrentDictionary<string, IDataStore>(StringComparer.OrdinalIgnoreCase);

        public Task<IDataStore> GetDataStore(string key)
        {
            var dataStore = _dataStores.GetOrAdd(key, new InMemoryDataStore());
            return Task.FromResult(dataStore);
        }
    }
}