using System;
using Gambot.Core;
using SlackNet;

namespace Gambot.IO.Slack
{
    public class SlackPerson : Person
    {
        public SlackPerson(User user)
        {
            Id = user.Id;
            Name = !String.IsNullOrWhiteSpace(user.Profile.DisplayName) ? user.Profile.DisplayName : user.Name;
            IsAdmin = user.IsAdmin;
            Mention = $"<@{user.Id}>";
        }

        public SlackPerson(Person person) : base(person) { }

        public SlackPerson WithPresence(Presence presence)
        {
            return new SlackPerson(this) { IsActive = presence == Presence.Active };
        }
    }
}