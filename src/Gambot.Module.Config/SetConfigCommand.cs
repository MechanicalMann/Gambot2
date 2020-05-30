using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;

namespace Gambot.Module.Config
{
    public class SetConfigCommand : ICommand
    {
        private readonly IConfig _config;

        public SetConfigCommand(IConfig config)
        {
            _config = config;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;

            var match = Regex.Match(message.Text, @"^set config (\w+) (.+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            if (!message.From.IsAdmin)
                return message.Respond($"I'm afraid I can't let you do that, {message.From.Mention}.");

            // TODO: Permissions
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            await _config.Set(key, value);
            return message.Respond($"Ok {message.From.Mention}, changed the value of {key} to \"{value}\"");
        }
    }
}