using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Factoid
{
    public class LiteralFactoidResponder : IResponder
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public LiteralFactoidResponder(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Respond(Message message)
        {
            if (!message.Addressed)
                return null;
            var match = Regex.Match(message.Text, @"^literal (.+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            var trigger = match.Groups[1].Value;

            var dataStore = await _dataStoreProvider.GetDataStore("Factoids");

            var values = await dataStore.GetAll(trigger);

            if (!values.Any())
                return message.Respond($"Sorry, {message.From}, but I don't know about \"{trigger}.\"");

            var result = String.Join(", ", values.Select(x => $"(#{x.Id}) {x.Value}"));

            return message.Respond($"{trigger}: {result}");
        }
    }
}