using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiscUtil.Linq;

namespace Gambot.Data.InMemory
{
    public class InMemoryDataStore : IDataStore
    {
        private readonly EditableLookup<string, string> _data = new EditableLookup<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Random _random = new Random();

        public Task<bool> Add(string key, string value)
        {
            if (_data.Contains(key, value))
                return Task.FromResult(false);
            _data.Add(key, value);
            return Task.FromResult(true);
        }

        public Task<DataStoreValue> Get(int id)
        {
            throw new NotImplementedException("In-memory data storage does not support access by ID.");
        }

        public Task<IEnumerable<DataStoreValue>> GetAll(string key)
        {
            return Task.FromResult(_data[key].Select(x => new DataStoreValue(-1, key, x)));
        }

        public Task<IEnumerable<string>> GetAllKeys()
        {
            return Task.FromResult(_data.Select(x => x.Key));
        }

        public Task<DataStoreValue> GetRandom(string key)
        {
            var values = _data[key].ToList();
            if (values.Count == 0)
                return Task.FromResult<DataStoreValue>(null);
            return Task.FromResult(new DataStoreValue(-1, key, values.ElementAt(_random.Next(0, values.Count))));
        }

        public Task<DataStoreValue> GetRandom()
        {
            var collection = _data.ToList();
            if (collection.Count == 0)
                return Task.FromResult<DataStoreValue>(null);
            var element = collection.ElementAt(_random.Next(0, collection.Count));
            var values = element.ToList();
            if (values.Count == 0)
                return Task.FromResult<DataStoreValue>(null);
            return Task.FromResult(new DataStoreValue(-1, element.Key, values.ElementAt(_random.Next(0, values.Count))));
        }

        public Task<DataStoreValue> GetSingle(string key)
        {
            var value = _data[key].SingleOrDefault();
            if (value == null)
                return Task.FromResult<DataStoreValue>(null);
            return Task.FromResult(new DataStoreValue(-1, key, value));
        }

        public Task<bool> Remove(string key, string value)
        {
            return Task.FromResult(_data.Remove(key, value));
        }

        public Task<bool> Remove(int id)
        {
            throw new NotImplementedException("In-memory data storage does not support access by ID.");
        }

        public Task<int> RemoveAll(string key)
        {
            var count = _data[key].Count();
            if (count > 0)
                _data.Remove(key);
            return Task.FromResult(count);
        }

        public Task<bool> SetSingle(string key, string value)
        {
            if (_data[key].Count() > 0)
                _data.Remove(key);
            _data.Add(key, value);
            return Task.FromResult(true);
        }

        public Task<int> GetCount(string key)
        {
            return Task.FromResult(_data[key].Count());
        }

        public Task<bool> Contains(string key, string value)
        {
            return Task.FromResult(_data[key].Any(val => val == value));
        }
    }
}