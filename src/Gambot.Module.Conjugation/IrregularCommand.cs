using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Conjugation
{
    public class IrregularCommand : ICommand
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public IrregularCommand(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;

            var match = Regex.Match(message.Text, "^irregular (stem|present|past|participle|gerund|plural) ([a-z][a-z0-9_-]*) (?:=>)?(.+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            var form = match.Groups[1].Value.ToLowerInvariant();
            var word = match.Groups[2].Value.ToLowerInvariant();
            var conjugation = match.Groups[3].Value.ToLowerInvariant();
            var key = $"{word}.{form}";

            var dataStore = await _dataStoreProvider.GetDataStore("Irregulars");

            var success = await dataStore.SetSingle(key, conjugation);
            if (!success)
                return message.Respond($"Sorry, {message.From.Mention}, something went wrong.");
            return message.Respond($"Ok, {message.From.Mention}.");
        }
    }
}