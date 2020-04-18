using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Variables
{
    public class AddVariableResponder : IResponder
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public AddVariableResponder(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Respond(Message message)
        {
            if (!message.Addressed)
                return null;
            
            var match = Regex.Match(message.Text, @"^add value ([a-z][a-z0-9_-]*) (.+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            var dataStore = await _dataStoreProvider.GetDataStore("Variables");
            var variable = match.Groups[1].Value.ToLowerInvariant();
            var value = match.Groups[2].Value;
            var added = await dataStore.Add(variable, value);

            if (!added)
                return message.Respond($"I already had it that way, {message.From}!");
            return message.Respond($"Ok, {message.From}.");
        }
    }
}