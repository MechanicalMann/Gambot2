using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Factoid
{
    public class AddFactoidResponder : IResponder
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public AddFactoidResponder(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Respond(Message message)
        {
            if (!message.Addressed)
                return null;

            var match = Regex.Match(message.Text, @"^(.+) (<[^>]+>) (.+)$");
            if (!match.Success)
                return null;
            var dataStore = await _dataStoreProvider.GetDataStore("Factoids");

            var trigger = match.Groups[1].Value;
            var verb = match.Groups[2].Value;
            var response = match.Groups[3].Value;

            var added = await dataStore.Add(trigger, $"{verb} {response}");

            if (!added)
                return message.Respond($"I already knew that, {message.From}!");
            return message.Respond($"Ok, {message.From}.");
        }
    }
}