using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.People
{
    public class AddPronounCommand : ICommand
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public AddPronounCommand(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;
            
            var match = Regex.Match(message.Text, @"^add pronoun ([a-z\/]+): ((?:\w+(?:[;,\s\/]+\b|$)){5})$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            var dataStore = await _dataStoreProvider.GetDataStore("Pronouns");
            
            var identifier = match.Groups[1].Value.ToLowerInvariant();
            if ((await dataStore.GetSingle(identifier)) != null)
                return message.Respond($"I'm sorry, {message.From.Mention}, but I already know some pronouns for {identifier}.");
            
            var pronouns = Regex.Split(match.Groups[2].Value.ToLowerInvariant(), @"[;,\s\/]+");

            await dataStore.SetSingle(identifier, String.Join(";", pronouns));

            return message.Respond($"Ok, {message.From.Mention}.");
        }
    }
}