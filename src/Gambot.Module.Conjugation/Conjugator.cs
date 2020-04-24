using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Conjugation
{
    public class Conjugator
    {
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly IDataStore _irregularStore;

        private static List<PatternReplacement> _pluralPatterns = new List<PatternReplacement>
        {
            // families of irregular plurals
            new PatternReplacement(@"(.*)man$", "$1men"),
            new PatternReplacement(@"(.*[ml])ouse$", "$1ice"),
            new PatternReplacement(@"(.*)goose$", "$1geese"),
            new PatternReplacement(@"(.*)tooth$", "$1teeth"),
            new PatternReplacement(@"(.*)foot$", "$1feet"),

            // unassimilated imports
            new PatternReplacement(@"(.*)ceps$", "$1ceps"),
            new PatternReplacement(@"(.*)zoon$", "$1zoa"),
            new PatternReplacement(@"(.*[csx])is$", "$1es"),

            // incompletely assimilated imports
            new PatternReplacement(@"(.*)trix$", "$1trices"),
            new PatternReplacement(@"(.*)eau$", "$1eaux"),
            new PatternReplacement(@"(.*)ieu$", "$1ieux"),
            new PatternReplacement(@"(.{2,}[yia])nx$", "$1nges"),

            // singular nouns ending in "s" or other silibants
            new PatternReplacement(@"^(.*s)$", "$1es"),
            new PatternReplacement(@"^(.*[^z])(z)$", "$1zzes"),
            new PatternReplacement(@"^(.*)([cs]h|x|zz|ss)$", "$1$2es"),

            // f -> ves
            new PatternReplacement(@"(.*[eao])lf", "$1lves"),
            new PatternReplacement(@"(.*[^d])eaf$", "$1eaves"),
            new PatternReplacement(@"(.*[nlw])ife$", "$1ives"),
            new PatternReplacement(@"(.*)arf$", "$1arves"),

            // y
            new PatternReplacement(@"(.*[aeiou])y$", "$1ys"),
            new PatternReplacement(@"(.*)y$", "$1ies"),

            // o
            new PatternReplacement(@"(.*[bcdfghjklmnpqrstvwxyz]o)$", "$1es"),
        };

        public Conjugator(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
            _irregularStore = dataStoreProvider.GetDataStore("Irregulars").Result;
        }

        private async Task<string> Stem(string verb)
        {
            var irregular = await _irregularStore.GetSingle($"{verb}.stem");
            if (irregular != null)
                return irregular.Value;

            // If the verb ends in CVC, we duplicate the last consonant before conjugating
            var match = Regex.Match(verb, @"[bcdfghjklmnpqrstvwxyz][aeiou][bcdfghjklmnpqrstv]$", RegexOptions.IgnoreCase);
            if (match.Success)
                return String.Concat(verb, verb.Last());

            return verb;
        }

        public async Task<string> Past(string verb)
        {
            verb = verb.ToLowerInvariant();
            var irregular = await _irregularStore.GetSingle($"{verb}.past");
            if (irregular != null)
                return irregular.Value;

            var stem = await Stem(verb);
            var past = $"{verb}ed";
            // Cy + ed = Cied (eg copy -> copied but not stay -> staied)
            past = Regex.Replace(past, @"([bcdfghjklmnpqrstvwxyz])yed", "$1ied");
            past = Regex.Replace(past, @"eed$", "ed");

            return past;
        }

        public async Task<string> Participle(string verb)
        {
            verb = verb.ToLowerInvariant();
            var irregular = await _irregularStore.GetSingle($"{verb}.participle");
            if (irregular != null)
                return irregular.Value;
            // For regular verbs the past participle is just the standard past form
            return await Past(verb);
        }

        public async Task<string> Gerund(string verb)
        {
            verb = verb.ToLowerInvariant();
            var irregular = await _irregularStore.GetSingle($"{verb}.gerund");
            if (irregular != null)
                return irregular.Value;

            var stem = await Stem(verb);
            var gerund = $"{stem}ing";
            gerund = Regex.Replace(gerund, @"(.[bcdfghjklmnpqrstvwxyz])eing$", "$1ing");
            gerund = Regex.Replace(gerund, @"ieing$", "ying"); // Not sure about this (sortying?)
            return gerund;
        }

        public async Task<string> ThirdPerson(string verb)
        {
            verb = verb.ToLowerInvariant();

            var irregular = await _irregularStore.GetSingle($"{verb}.present");
            if (irregular != null)
                return irregular.Value;

            // Stems ending in sibilants take an extra syllable to carry the s
            if (Regex.IsMatch(verb, @"(?:[xs]|[cs]h)$"))
                return $"{verb}es";
            var sform = $"{verb}s";
            // Cys -> Cies
            sform = Regex.Replace(verb, @"(.[bcdfghjklmnpqrstvwxyz])ys$", "$1ies");
            return sform;
        }

        public async Task<string> Plural(string noun)
        {
            noun = noun.ToLowerInvariant();

            var irregular = await _irregularStore.GetSingle($"{noun}.plural");
            if (irregular != null)
                return irregular.Value;

            // Iterate through common classes of strange plural forms
            string plural;
            foreach (var pr in _pluralPatterns)
            {
                plural = Regex.Replace(noun, pr.Pattern, pr.Replacement);
                if (plural != noun)
                    return plural; // As soon as we find a plural form, return it
            }

            // Everything else
            return $"{noun}s";
        }

        private class PatternReplacement
        {
            public string Pattern { get; set; }
            public string Replacement { get; set; }

            public PatternReplacement(string pattern, string replacement)
            {
                Pattern = pattern;
                Replacement = replacement;
            }
        }
    }
}