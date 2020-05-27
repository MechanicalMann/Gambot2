using System.Linq;
using System;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Quotes
{
    public class QuoteVariableHandler : IVariableHandler
    {
        private readonly Random _random = new Random();

        private readonly IDataStoreProvider _dataStoreProvider;

        public QuoteVariableHandler(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<string> GetValue(string variable, Message context)
        {
            if (!variable.Equals("quote", StringComparison.OrdinalIgnoreCase))
                return null;

            var quoteStore = await _dataStoreProvider.GetDataStore("Quotes");
            var allUsers = await quoteStore.GetAllKeys();
            if (!allUsers.Any())
                return null;

            var user = allUsers.ElementAt(_random.Next(allUsers.Count()));
            var quote = await quoteStore.GetRandom(user);
            return quote.Value;
        }
    }
}