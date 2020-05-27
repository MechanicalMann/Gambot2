using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Alea;
using Alea.Exceptions;
using Gambot.Core;

namespace Gambot.Module.Dice
{
    using Dice = Alea.Dice;

    public class RollResponder : IResponder
    {
        private readonly ILogger _log;

        public RollResponder(ILogger log)
        {
            _log = log;
        }

        public Task<Response> Respond(Message message)
        {
            var match = Regex.Match(message.Text, @"^roll (\d?d\d+.*)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return Task.FromResult<Response>(null);

            var dice = match.Groups[1].Value;
            try
            {
                var result = Dice.Roll(dice).ToString("0.##");
                var n = result.StartsWith("8") || result == "18" ? "n" : "";
                return Task.FromResult(message.Respond($"{message.From.Mention}, you rolled a{n} {result}!"));
            }
            catch (AleaException ex)
            {
                _log.Warn(ex, $"An error occurred when attempting to roll dice \"{dice}\": {ex.Message}");
                return Task.FromResult(message.Respond($"I can't roll that, {message.From.Mention}!"));
            }
        }
    }
}