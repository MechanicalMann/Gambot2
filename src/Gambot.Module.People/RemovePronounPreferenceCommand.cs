using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.People
{
    public class RemovePronounPreferenceCommand : ICommand
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public RemovePronounPreferenceCommand(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;
            
            var match = Regex.Match(message.Text, @"I (?:do not|don't|no longer) go by (\w+)\.?$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            
            var dataStore = await _dataStoreProvider.GetDataStore("PronounPreferences");

            var success = await dataStore.Remove(message.From.Id, match.Groups[1].Value);

            if (!success)
                return message.Respond($"I already had it that way, {message.From.Mention}!");
            return message.Respond($"Ok, {message.From.Mention}.");
        }
    }
}