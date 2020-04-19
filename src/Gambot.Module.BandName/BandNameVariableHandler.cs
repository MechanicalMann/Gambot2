using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.BandName
{
    public class BandNameVariableHandler : IVariableHandler
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public BandNameVariableHandler(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<string> GetValue(string variable, Message context)
        {
            var match = Regex.Match(variable, @"(?:band|tla)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            var dataStore = await _dataStoreProvider.GetDataStore("BandNames");
            return (await dataStore.GetRandom())?.Value;
        }
    }
}