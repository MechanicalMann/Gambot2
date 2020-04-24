using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Conjugation
{
    public class VariableTypeCommand : ICommand
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public VariableTypeCommand(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;
            var match = Regex.Match(message.Text, @"^var ([a-z][a-z0-9_-]*) (?:(?:is (?:a|type))|type) (var|verb|noun)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            var variable = match.Groups[1].Value.ToLowerInvariant();
            var type = match.Groups[2].Value.ToLowerInvariant();

            var dataStore = await _dataStoreProvider.GetDataStore("VariableTypes");
            var success = await dataStore.SetSingle(variable, type);
            if (!success)
                return message.Respond($"Sorry, {message.From}, I'm afraid I can't do that.");
            return message.Respond($"Ok, {message.From}.");
        }
    }
}