using System.Linq;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Bot
{
    public class DataStoreConfig : IConfig
    {
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly ILogger _log;

        public DataStoreConfig(IDataStoreProvider dataStoreProvider, ILogger log)
        {
            _dataStoreProvider = dataStoreProvider;
            _log = log;
        }

        public async Task<string> Get(string key, string defaultValue = null)
        {
            _log.Trace($"Getting config value for {key}");
            var dataStore = await _dataStoreProvider.GetDataStore("Config");
            var value = (await dataStore.GetAll(key)).FirstOrDefault();
            if (value == null)
            {
                _log.Info($"No config value set for {key}, defaulting to {defaultValue ?? "null"}");
                return defaultValue;
            }
            _log.Trace($"Successfully got config value \"{value.Value}\" for {key}");
            return value.Value;
        }

        public async Task Set(string key, string value)
        {
            _log.Trace($"Updating config value {key} to \"{value}\"");
            var dataStore = await _dataStoreProvider.GetDataStore("Config");
            await dataStore.RemoveAll(key);
            var success = await dataStore.Add(key, value);
            if (!success)
                _log.Warn($"Unable to update config value for {key}");
            else
                _log.Trace($"Updated config value {key} to \"{value}\"");
        }
    }
}