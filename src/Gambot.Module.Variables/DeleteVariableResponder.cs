using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Variables
{
    public class DeleteVariableResponder : IResponder
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public DeleteVariableResponder(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Respond(Message message)
        {
            if (!message.Addressed)
                return null;
            var match = Regex.Match(message.Text, @"^delete var ([a-z][a-z0-9_-]*)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            
            var dataStore = await _dataStoreProvider.GetDataStore("Variables");

            var variable = match.Groups[1].Value.ToLowerInvariant();
            var values = await dataStore.RemoveAll(variable);

            if (values == 0)
                return message.Respond($"There's no such variable, {message.From}!");
            if (values == 1)
                return message.Respond($"Ok, {message.From}, deleted variable \"{variable}\" and its value.");
            return message.Respond($"Ok, {message.From}, deleted variable \"{variable}\" and its {values} values.");
        }
    }
}