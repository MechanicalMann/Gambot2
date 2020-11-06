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
    readonly struct VariableReference {
        public VariableReference(Match match) {
            Variable = match.Groups[2].Value.ToLowerInvariant();
            Key = match.Groups[3].Success
                ? match.Groups[3].Value.ToLowerInvariant()
                : null;
        }
        public VariableReference(string variable, string key) {
            Variable = variable;
            Key = key;
        }
        public string Variable { get; }
        public string Key { get; }
    }

    public class VariableTransformer : ITransformer
    {
        private readonly Regex _variableRegex = new Regex(@"((?:^| )an? )?\$([a-z][a-z0-9_]*(?<!_))(?:\[([^\]]+)\])?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly char[] _vowels = new [] { 'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U' };

        private readonly ICollection<IVariableHandler> _variableHandlers;

        public VariableTransformer(ICollection<IVariableHandler> variableHandlers)
        {
            _variableHandlers = variableHandlers;
        }

        public async Task<Response> Transform(Response response)
        {
            // Shitty recursion for nested variable references
            while (response.Text.Contains("$"))
            {
                string substitution = await Substitute(response.Text, response.Message);
                if (substitution == response.Text)
                    break; // We've run out of variables to replace
                response.Text = substitution;
            }
            return response;
        }

        private async Task<string> Substitute(string text, Message context)
        {
            var replacements = new Dictionary<VariableReference, string>();
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

        private async Task<string> Substitute(Match match, Message context, IDictionary<VariableReference, string> replacements)
        {
            var variable = match.Groups[2].Value;
            var key = variable.ToLowerInvariant();
            var substitution = match.Value;

            var replaced = false;

            var reference = new VariableReference(match);
            if (context.Variables.ContainsKey(key))
            {
                substitution = context.Variables[key];
                replaced = true;
            }
            else if (reference.Key != null && replacements.ContainsKey(reference))
            {
                substitution = replacements[reference];
                replaced = true;
            }
            else
            {
                foreach (var handler in _variableHandlers)
                {
                    var value = await handler.GetValue(key, context);
                    if (value != null)
                    {
                        substitution = value;
                        if (reference.Key != null)
                            replacements[reference] = substitution;
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
            substitution = (leadingSpace ? " " : "")
                + MatchCase(an.Trim(), startsWithVowel ? "an" : "a")
                + " "
                + substitution;
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
                        Char.ToUpper(word[0]).ToString()
                        + word.Substring(1)));
            return destination;
        }
    }
}