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
        private readonly IDataDumper _dataDumper;

        public ListVariableCommand(IDataStoreProvider dataStoreProvider, IDataDumper dataDumper)
        {
            _dataDumper = dataDumper;
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

            if (values.Count() > 10 || values.Sum(x => x.Value.Length) > 500)
            {
                var url = await _dataDumper.Dump("Variables", variable);
                return message.Respond($"${variable}: {url}");
            }

            var list = String.Join(", ", values.Select(v => $"(#{v.Id}) {v.Value}"));
            return message.Respond($"{variable}: {list}");
        }
    }
}