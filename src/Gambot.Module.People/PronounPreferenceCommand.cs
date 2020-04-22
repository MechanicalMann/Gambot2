using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.People
{
    public class PronounPreferenceCommand : ICommand
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public PronounPreferenceCommand(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;
            var match = Regex.Match(message.Text, @"^I go by ([\w/]+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            var pronouns = await _dataStoreProvider.GetDataStore("Pronouns");
            var key = match.Groups[1].Value.ToLowerInvariant();

            var pronoun = (await pronouns.GetAll(key)).SingleOrDefault();
            if (pronoun == null)
                return message.Respond($"Sorry, {message.From}, but I don't know that pronoun.");

            var preferences = await _dataStoreProvider.GetDataStore("PronounPreferences");
            await preferences.RemoveAll(message.From);
            await preferences.Add(message.From, pronoun.Key);
            return message.Respond($"Ok, {message.From}.");
        }
    }
}