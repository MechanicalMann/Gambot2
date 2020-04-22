using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.People
{
    public class PronounVariableHandler : IVariableHandler
    {
        private static string[] _subjective = new string[] { "subjective", "they", "shehe", "heshe" };
        private static string[] _objective = new string[] { "objective", "them", "herhim", "himher" };
        private static string[] _determiner = new string[] { "determiner", "their", "herhis", "hisher" };
        private static string[] _possessive = new string[] { "possessive", "theirs", "hershis", "hishers" };
        private static string[] _reflexive = new string[] { "reflexive", "themself", "herhimself", "himherself", "herselfhimself", "himselfherself" };

        private readonly Dictionary<PartOfSpeech, string[]> _mapping = new Dictionary<PartOfSpeech, string[]>
        { { PartOfSpeech.Subjective, _subjective },
            { PartOfSpeech.Objective, _objective },
            { PartOfSpeech.Determiner, _determiner },
            { PartOfSpeech.Possessive, _possessive },
            { PartOfSpeech.Reflexive, _reflexive },
        };

        private readonly IDataStoreProvider _dataStoreProvider;

        public PronounVariableHandler(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        private async Task<string> GetPronoun(PartOfSpeech partOfSpeech, Message context)
        {
            var history = await _dataStoreProvider.GetDataStore("PersonHistory");
            var lastKnown = (await history.GetAll(context.Channel)).FirstOrDefault()?.Value;

            var preferences = await _dataStoreProvider.GetDataStore("PronounPreferences");
            var pronouns = await _dataStoreProvider.GetDataStore("Pronouns");

            var gender = lastKnown != null ? (await preferences.GetAll(lastKnown)).FirstOrDefault()?.Value ?? "they" : "they";

            var list = (await pronouns.GetAll(gender)).Single().Value;

            return list.Split(';') [(int) partOfSpeech];
        }

        public async Task<string> GetValue(string variable, Message context)
        {
            foreach (var map in _mapping)
                if (map.Value.Contains(variable, StringComparer.OrdinalIgnoreCase))
                    return await GetPronoun(map.Key, context);
            return null;
        }
    }

    internal enum PartOfSpeech
    {
        Subjective = 0,
        Objective = 1,
        Determiner = 2,
        Possessive = 3,
        Reflexive = 4,
    }
}