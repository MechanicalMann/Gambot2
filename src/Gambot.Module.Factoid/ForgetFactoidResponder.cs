using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Factoid
{
    public class ForgetFactoidResponder : IResponder
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public ForgetFactoidResponder(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Respond(Message message)
        {
            if (!message.Addressed)
                return null;
            var match = Regex.Match(message.Text, @"^forget #([0-9]+)$", RegexOptions.IgnoreCase);
            int id;
            if (!match.Success || !Int32.TryParse(match.Groups[1].Value, out id))
                return null;

            var dataStore = await _dataStoreProvider.GetDataStore("Factoids");

            var removed = await dataStore.Remove(id);

            if (!removed)
                return message.Respond($"I didn't know anything by that number, {message.From}!");
            return message.Respond($"Ok, {message.From}.");
        }
    }
}