using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Conjugation
{
    public class ConjugatedVariableHandler : IVariableHandler
    {
        private readonly IDataStoreProvider _dataStoreProvider;

        private readonly Conjugator _conjugator;

        public ConjugatedVariableHandler(IDataStoreProvider dataStoreProvider, Conjugator conjugator)
        {
            _dataStoreProvider = dataStoreProvider;
            _conjugator = conjugator;
        }

        public async Task<string> GetValue(string variable, Message context)
        {
            var variableTypes = await _dataStoreProvider.GetDataStore("VariableTypes");

            var match = Regex.Match(variable, @"(.+)(?:(s)|(ed)|(en)|(ing))$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            variable = match.Groups[1].Value;

            var type = await variableTypes.GetSingle(variable);
            if (type == null)
                return null;

            var dataStore = await _dataStoreProvider.GetDataStore("Variables");

            if (match.Groups[2].Success && type.Value == "noun")
            {
                var noun = await dataStore.GetRandom(variable);
                if (noun == null)
                    return null;
                return await _conjugator.Plural(noun.Value);
            }

            var verb = await dataStore.GetRandom(variable);
            if (verb == null)
                return null;
            if (match.Groups[2].Success && type.Value == "verb")
                return await _conjugator.ThirdPerson(verb.Value);
            if (match.Groups[3].Success && type.Value == "verb")
                return await _conjugator.Past(verb.Value);
            if (match.Groups[4].Success && type.Value == "verb")
                return await _conjugator.Participle(verb.Value);
            if (match.Groups[5].Success && type.Value == "verb")
                return await _conjugator.Gerund(verb.Value);

            return null;
        }
    }
}