using Gambot.Core;
using SlackConnector.Models;

namespace Gambot.IO.Slack
{
    public class SlackPerson : Person
    {
        public SlackPerson(SlackUser user)
        {
            Id = user.Id;
            Name = user.Name;
            Mention = user.FormattedUserId;
            // SlackConnector does not provide an easy way to determine whether
            // a user is active or not.  The SlackMessenger should handle this.
            IsAdmin = user.IsAdmin;
        }
    }
}