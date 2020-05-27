using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Quotes
{
    public class GetQuoteCommand : ICommand
    {
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly IPersonProvider _personProvider;

        public GetQuoteCommand(IDataStoreProvider dataStoreProvider, IPersonProvider personProvider)
        {
            _dataStoreProvider = dataStoreProvider;
            _personProvider = personProvider;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;
            
            var match = Regex.Match(message.Text, @"^quote (\S+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            
            var target = match.Groups[1].Value;
            var person = await _personProvider.GetPersonByName(message.Channel, target);
            if (person == null)
                return message.Respond($"Sorry, {message.From.Mention}, I don't know anyone named \"{target}.\"");
            
            var quoteStore = await _dataStoreProvider.GetDataStore("Quotes");
            var quote = await quoteStore.GetRandom(person.Id);
            if (quote == null)
                return message.Respond($"Sorry, {person.Name} has not said anything quote-worthy yet.");
            
            return message.Respond($"<{person.Name}> {quote.Value}");
        }
    }
}