using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.BandName
{
    public class ExpandBandNameResponder : IResponder
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public ExpandBandNameResponder(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Respond(Message message)
        {
            var match = Regex.Match(message.Text, @"^([A-Z]{3})$");
            if (!match.Success)
                return null;

            var dataStore = await _dataStoreProvider.GetDataStore("BandNames");
            var acronym = match.Groups[1].Value;

            var bandName = await dataStore.GetRandom(acronym);
            if (bandName == null)
                return null;
            return message.Respond(bandName.Value);
        }
    }
}