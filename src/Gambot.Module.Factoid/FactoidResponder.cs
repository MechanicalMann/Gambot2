using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Factoid
{
    public class FactoidResponder : IResponder
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public FactoidResponder(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }
        public async Task<Response> Respond(Message message)
        {
            if (!message.Addressed && message.Text.Length < 6)
                return null;
            var trigger = message.Text;

            var dataStore = await _dataStoreProvider.GetDataStore("Factoids");

            var reply = await dataStore.GetRandom(trigger);
            if (reply == null)
                return null;

            var factoid = ParseFactoid(trigger, reply.Value);

            if (factoid.Verb == "reply")
                return message.Respond(factoid.Response);
            if (factoid.Verb == "action")
                return message.Respond(factoid.Response, true);

            return message.Respond($"{trigger} {factoid.Verb} {factoid.Response}");
        }

        private static Factoid ParseFactoid(string trigger, string partial)
        {
            var match = Regex.Match(partial, @"^<(.+?)> (.+)");
            if (!match.Success)
                return null;
            return new Factoid
            {
                Trigger = trigger,
                Verb = match.Groups[1].Value,
                Response = match.Groups[2].Value,
            };
        }
    }
}