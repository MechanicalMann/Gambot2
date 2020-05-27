using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Quotes
{
    public class AddQuoteCommand : ICommand
    {
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly IPersonProvider _personProvider;

        public AddQuoteCommand(IDataStoreProvider dataStoreProvider, IPersonProvider personProvider)
        {
            _dataStoreProvider = dataStoreProvider;
            _personProvider = personProvider;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;

            var match = Regex.Match(message.Text, @"^remember (\S+) (.+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            
            var target = match.Groups[1].Value;
            var search = match.Groups[2].Value;

            var person = await _personProvider.GetPersonByName(message.Channel, target);
            if (person == null)
                return message.Respond($"Sorry, {message.From.Mention}, I don't know anyone named \"{target}.\"");
            if (person.Id == message.From.Id)
                return message.Respond($"Sorry, {message.From.Mention}, but you can't quote yourself.");
            
            var history = await message.Messenger.GetMessageHistory(message.Channel);
            var matchingMessage = history.Where(x => x.From.Id == person.Id && x.Text.IndexOf(search, 0) > -1).FirstOrDefault();

            if (matchingMessage == null)
                return message.Respond($"Sorry, {message.From.Mention}, I don't remember what {target} said about \"{search}.\"");
            
            var quoteStore = await _dataStoreProvider.GetDataStore("Quotes");
            await quoteStore.Add(person.Id, matchingMessage.Text);

            return message.Respond($"You got it, {message.From.Mention}.");
        }
    }
}