using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Variables
{
    public class RemoveVariableCommand : ICommand
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public RemoveVariableCommand(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;
            var match = Regex.Match(message.Text, @"^remove value (?:([a-z][a-z0-9_-]*) (.+)|#([0-9]+))$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            var dataStore = await _dataStoreProvider.GetDataStore("Variables");
            var removed = false;
            if (match.Groups[1].Success && match.Groups[2].Success)
                removed = await dataStore.Remove(match.Groups[1].Value.ToLowerInvariant(), match.Groups[2].Value);
            else
                removed = await dataStore.Remove(Int32.Parse(match.Groups[3].Value));
            
            if (!removed)
                return message.Respond($"There's no such value, {message.From}!");
            return message.Respond($"Ok, {message.From}");
        }
    }
}