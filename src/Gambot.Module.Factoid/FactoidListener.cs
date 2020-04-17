using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Factoid
{
    public class FactoidListener : IListener
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        public FactoidListener(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task Listen(Message message)
        {
            var match = Regex.Match(message.Text, @"^(.{0,42})\s+(is(?: also)?|are)\s+(.+)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return;
            
            var dataStore = await _dataStoreProvider.GetDataStore("Factoids");
            
            var trigger = match.Groups[1].Value;
            var verb = match.Groups[2].Value;
            var tidbit = match.Groups[3].Value;

            await dataStore.Add(trigger, $"<{verb}> {tidbit}");
        }
    }
}