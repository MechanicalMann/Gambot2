using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Variables
{
    public class BasicVariableHandler : IVariableHandler
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public BasicVariableHandler(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<string> GetValue(string variable, Message context)
        {
            var dataStore = await _dataStoreProvider.GetDataStore("Variables");
            return (await dataStore.GetRandom(variable))?.Value;
        }
    }
}