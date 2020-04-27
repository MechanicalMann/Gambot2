using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.BandName
{
    public class AddBandNameResponder : IResponder
    {
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly IConfig _config;
        private readonly Random _random = new Random();

        public AddBandNameResponder(IDataStoreProvider dataStoreProvider, IConfig config)
        {
            _dataStoreProvider = dataStoreProvider;
            _config = config;
        }

        public async Task<Response> Respond(Message message)
        {
            var match = Regex.Match(message.Text, @"^([a-z]\w*)\s+([a-z]\w*)\s+([a-z]\w*)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            var bandChance = Int32.Parse(await _config.Get("PercentChanceOfBandName", "5"));

            if (_random.Next(0, 100) > bandChance)
                return null;

            var bandNameDataStore = await _dataStoreProvider.GetDataStore("BandNames");

            var words = new [] { match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value };

            var expanded = String.Join(" ", words);
            var tla = String.Concat(words.Select(x => x.First())).ToUpperInvariant();

            if (!await bandNameDataStore.Add(tla, expanded))
                return null;

            var factoids = await _dataStoreProvider.GetDataStore("Factoids");
            var reply = (await factoids.GetRandom("band name reply"))?.Value ?? "\"$band\" would be a cool name for a band.";

            reply = reply.Replace("<reply> ", "");
            reply = Regex.Replace(reply, @"\$(?:band|tla)", expanded, RegexOptions.IgnoreCase);

            return message.Respond(reply);
        }
    }
}