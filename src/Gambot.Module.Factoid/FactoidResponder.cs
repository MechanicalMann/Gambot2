using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Factoid
{
    public class FactoidResponder : IResponder
    {
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly IConfig _config;
        private readonly Random _random;

        public FactoidResponder(IDataStoreProvider dataStoreProvider, IConfig config)
        {
            _dataStoreProvider = dataStoreProvider;
            _config = config;
            _random = new Random();
        }
        public async Task<Response> Respond(Message message)
        {
            var triggerLength = Int32.Parse(await _config.Get("FactoidTriggerLength", "6"));

            if (!message.Addressed && message.Text.Length < triggerLength)
                return null;
            var trigger = message.Text;

            var dataStore = await _dataStoreProvider.GetDataStore("Factoids");
            var historyStore = await _dataStoreProvider.GetDataStore("FactoidHistory");
            var aliases = new HashSet<string>();

            while (true)
            {
                var reply = await dataStore.GetRandom(trigger);
                if (reply == null)
                    return null;

                var factoid = ParseFactoid(trigger, reply.Value);

                if (factoid.Verb == "alias")
                {
                    if (!aliases.Add(trigger))
                        return message.Respond("Oh no! There's a factoid that resolves to a circular reference!");
                    trigger = factoid.Response;
                    continue;
                }

                await historyStore.SetSingle(message.Channel, $"(#{reply.Id}) \"{factoid.ToString()}\"");

                if (_random.Next(100) > factoid.ChanceToTrigger)
                    return null;

                if (factoid.Verb == "reply")
                    return message.Respond(factoid.Response);
                if (factoid.Verb == "action")
                    return message.Respond(factoid.Response, true);

                return message.Respond($"{trigger} {factoid.Verb} {factoid.Response}");
            }
        }

        private static Factoid ParseFactoid(string trigger, string partial)
        {
            // Matches `<verb> response`, or `<verb|n%> response`.
            // In the former case, group 2 (the percentage) will be "", which defaults to 100%.
            var match = Regex.Match(partial, @"^<(.+?)(?:\|(?!.*\|)(.*?)%)?> (.+)");
            if (!match.Success)
                return null;

            var factoid = new Factoid
            {
                Trigger = trigger,
                Verb = match.Groups[1].Value,
                ChanceToTrigger = 100.0m,
                Response = match.Groups[3].Value,
            };

            if (!String.IsNullOrEmpty(match.Groups[2].Value))
                factoid.ChanceToTrigger = Decimal.Parse(match.Groups[2].Value);

            return factoid;
        }
    }
}