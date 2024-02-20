using Gambot.Core;
using SlackNet;

namespace Gambot.IO.Slack
{
    public class SlackPerson : Person
    {
        public SlackPerson(User user)
        {
            Id = user.Id;
            Name = user.Name;
            IsAdmin = user.IsAdmin;
            Mention = $"<@{user.Id}>";
        }
    }
}