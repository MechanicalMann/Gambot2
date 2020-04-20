using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Factoid
{
    public class FactoidHistoryCommand : ICommand
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public FactoidHistoryCommand(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;

            var match = Regex.Match(message.Text, @"^what was that\??$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            var dataStore = await _dataStoreProvider.GetDataStore("FactoidHistory");
            var lastFactoid = (await dataStore.GetAll(message.Channel)).FirstOrDefault();

            if (lastFactoid == null)
                return message.Respond("¯\\_(ツ)_/¯");

            return message.Respond($"{message.From}, that was {lastFactoid.Value}");
        }
    }
}