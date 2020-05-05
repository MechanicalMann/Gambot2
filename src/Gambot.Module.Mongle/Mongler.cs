using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Mongle
{
    public class Mongler : ITransformer
    {
        private static char[] fuckupKeys = { 'a', 's', 'd', 'g', 'h', 'f', 'j', 'k', 'l' };

        private readonly IConfig _config;
        private readonly Random _random;
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly ILogger _log;

        public Mongler(IConfig config, IDataStoreProvider dataStoreProvider, ILogger log)
        {
            _log = log;
            _config = config;
            _dataStoreProvider = dataStoreProvider;
            _random = new Random();
        }

        public async Task<Response> Transform(Response response)
        {
            var chance = Int32.Parse(await _config.Get("PercentChanceOfMongling", "1"));
            if (_random.Next(100) > chance)
                return response;

            _log.Info("It's time.");
            var dataStore = await _dataStoreProvider.GetDataStore("Monglings");

            var chanceSwaps = Int32.Parse(await _config.Get("PercentChanceOfMongledSwaps", "66"));
            var chanceSpace = Int32.Parse(await _config.Get("PercentChanceOfMongledSpaces", "75"));
            var chanceChars = Int32.Parse(await _config.Get("PercentChanceOfMongledLetters", "99"));

            var text = response.Text.Trim();
            if (_random.Next(100) < chanceSwaps)
            {
                var swaps = await dataStore.GetAllKeys();
                foreach (var swap in swaps)
                {
                    var match = Regex.Match(text, $@"\b{swap}\b", RegexOptions.IgnoreCase);
                    if (match.Success && _random.Next(100) < chanceSwaps)
                    {
                        var replacement = (await dataStore.GetRandom(swap))?.Value;
                        if (replacement == null)
                            continue;
                        text = Regex.Replace(text, $@"\b{swap}\b", replacement);
                        chanceSwaps = Math.Min((int) (chanceSwaps * 1.1), 99); // Increase chance of word swaps with each swapped word
                    }
                }
            }

            var space = text.IndexOf(' ');
            if (space > 0 && _random.Next(100) < chanceSpace)
            {
                while (_random.Next(text.Length * 2) > space)
                {
                    var next = text.IndexOf(' ', space + 1);
                    if (next < 0 || next == text.Length - 1)
                        break;
                    space = next;
                }
                var sb = new StringBuilder()
                    .Append(text.Substring(0, space))
                    .Append(text[space + 1])
                    .Append(" ")
                    .Append(text.Substring(space + 2));
                text = sb.ToString();
            }

            var idx = text.IndexOfAny(fuckupKeys);
            if (idx >= 0 && _random.Next(100) < chanceChars)
            {
                // More likely to glitch on later characters
                while (_random.Next(text.Length * 2) > idx)
                {
                    var next = text.IndexOfAny(fuckupKeys, idx + 1);
                    if (next < 0)
                        break;
                    idx = next;
                }

                var toRepeat = text[idx];
                var sb = new StringBuilder()
                    .Append(text.Substring(0, idx + 1));
                var repeats = _random.Next(4, 20);
                sb.Append(toRepeat, repeats);

                // There's a chance the rest of the message is entirely lost
                if (_random.Next(100) > chanceSwaps)
                    sb.Append(text.Substring(idx + 1));

                text = sb.ToString();
            }

            response.Text = text;
            return response;
        }
    }
}