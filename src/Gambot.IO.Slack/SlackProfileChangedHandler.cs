using System;
using System.Threading.Tasks;
using SlackNet;
using SlackNet.Events;

namespace Gambot.IO.Slack
{
    public class OnProfileChangedEventArgs : EventArgs
    {
        public UserProfileChanged Event { get; set; }
    }

    public delegate Task OnProfileChangedEventHandler(object sender, OnProfileChangedEventArgs e);

    public class SlackProfileChangedHandler : IEventHandler<UserProfileChanged>
    {
        public event OnProfileChangedEventHandler OnProfileChanged;

        public async Task Handle(UserProfileChanged slackEvent)
        {
            await OnProfileChanged?.Invoke(this, new OnProfileChangedEventArgs { Event = slackEvent });
        }
    }
}