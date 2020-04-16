using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;

namespace Gambot.Module.Say
{
    public class SayResponder : IResponder
    {
        public Task<Response> Respond(Message message)
        {
            Match match;
            Response response = null;
            if (message.Addressed)
            {
                match = Regex.Match(message.Text, "say \"(.+)\"");
                if (match.Success)
                {
                    response = message.Respond(match.Groups[1].Value);
                }
            }
            else
            {
                match = Regex.Match(message.Text, @"^say (\S)([^.?!]+)[.?!]*$");
                if (match.Success)
                {
                    response = message.Respond($"{match.Groups[1].Value.ToUpper()}{match.Groups[2].Value}!");
                }
            }
            return Task.FromResult(response);
        }
    }
}