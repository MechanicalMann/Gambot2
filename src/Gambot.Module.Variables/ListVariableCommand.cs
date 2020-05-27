using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Variables
{
    public class ListVariableCommand : ICommand
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public ListVariableCommand(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;
            var match = Regex.Match(message.Text, @"^list var ([a-z][a-z0-9_-]*)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            var dataStore = await _dataStoreProvider.GetDataStore("Variables");
            var variable = match.Groups[1].Value.ToLowerInvariant();
            var values = await dataStore.GetAll(variable);

            if (!values.Any())
                return message.Respond($"There's no such variable, {message.From.Mention}!");
            
            var list = String.Join(", ", values.Select(v => $"(#{v.Id}) {v.Value}"));
            return message.Respond($"{variable}: {list}");
        }
    }
}