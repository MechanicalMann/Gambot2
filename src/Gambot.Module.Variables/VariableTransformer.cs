using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Variables
{
    public class VariableTransformer : ITransformer
    {
        private readonly Regex _variableRegex = new Regex(@"((?:^| )an? )?\$([a-z][a-z0-9_]*(?<!_))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly char[] _vowels = new [] { 'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U' };

        private readonly ICollection<IVariableHandler> _variableHandlers;
        private readonly IDataStoreProvider _dataStoreProvider;

        public VariableTransformer(ICollection<IVariableHandler> variableHandlers, IDataStoreProvider dataStoreProvider)
        {
            _variableHandlers = variableHandlers;
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Transform(Response response)
        {
            while (response.Text.Contains("$"))
                response.Text = await Substitute(response.Text, response.Message);
            return response;
        }

        private async Task<string> Substitute(string text, Message context)
        {
            var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var sb = new StringBuilder();
            var i = 0;

            foreach (Match match in _variableRegex.Matches(text))
            {
                sb.Append(text, i, match.Index - i);
                sb.Append(await Substitute(match, context, replacements));
                i = match.Index + match.Length;
            }

            sb.Append(text, i, text.Length - i);
            return sb.ToString();
        }

        private async Task<string> Substitute(Match match, Message context, Dictionary<string, string> replacements)
        {
            var variable = match.Groups[2].Value;
            var substitution = match.Value;

            var replaced = false;

            if (replacements.ContainsKey(variable))
            {
                substitution = replacements[variable];
                replaced = true;
            }
            else
            {
                foreach (var handler in _variableHandlers)
                {
                    var value = await handler.GetValue(variable, context);
                    if (value != null)
                    {
                        substitution = value;
                        replacements[variable] = substitution;
                        replaced = true;
                        break;
                    }
                }
            }

            if (!replaced)
                return substitution; // Didn't replace, leave the original

            substitution = MatchCase(variable, substitution);

            if (!match.Groups[1].Success)
                return substitution;

            // Handle a/an
            var an = match.Groups[1].Value;
            var leadingSpace = an.StartsWith(" ");
            var startsWithVowel = _vowels.Contains(substitution[0]);
            substitution = (leadingSpace ? " " : "") +
                MatchCase(an.Trim(), startsWithVowel ? "an" : "a") +
                " " +
                substitution;
            return substitution;
        }

        private static string MatchCase(string sourceCase, string destination)
        {
            if (sourceCase.Length > 1 && sourceCase.All(c => !Char.IsLetter(c) || Char.IsUpper(c)))
                return destination.ToUpper();
            if (Char.IsUpper(sourceCase[0]))
                return String.Join(" ",
                    destination.Split(' ').Where(word => !String.IsNullOrWhiteSpace(word))
                    .Select(word =>
                        Char.ToUpper(word[0]).ToString() +
                        word.Substring(1)));
            return destination;
        }
    }
}