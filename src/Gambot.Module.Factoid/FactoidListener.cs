using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Factoid
{
    public class FactoidListener : IListener
    {
        private readonly IConfig _config;
        private readonly Random _random;
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly ILogger _log;

        public FactoidListener(IDataStoreProvider dataStoreProvider, IConfig config, ILogger log)
        {
            _log = log;
            _config = config;
            _dataStoreProvider = dataStoreProvider;
            _random = new Random();
        }

        public async Task Listen(Message message)
        {
            var chance = Int32.Parse(await _config.Get("PercentChanceOfPassiveFactoid", "10"));
            if (_random.Next(100) > chance)
                return;

            var match = Regex.Match(message.Text, @"^(.{0,42})\s+(is(?: also)?|are)\s+([^?]+)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return;

            _log.Debug("Someone said a factoid!");

            var dataStore = await _dataStoreProvider.GetDataStore("Factoids");

            var trigger = match.Groups[1].Value;
            var verb = match.Groups[2].Value;
            var tidbit = match.Groups[3].Value;

            await dataStore.Add(trigger, $"<{verb}> {tidbit}");
        }
    }
}