using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;

namespace Gambot.Module.Config
{
    public class GetConfigCommand : ICommand
    {
        private readonly IConfig _config;

        public GetConfigCommand(IConfig config)
        {
            _config = config;
        }

        public async Task<Response> Handle(Message message)
        {
            if (!message.Addressed)
                return null;

            var match = Regex.Match(message.Text, @"^get config (\w+)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            var key = match.Groups[1].Value;
            var value = await _config.Get(key);

            if (value == null)
                return message.Respond($"No value has been set for {key} yet.");
            return message.Respond($"The current value for {key} is \"{value}\"");
        }
    }
}