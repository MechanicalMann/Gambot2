using System.Threading.Tasks;
using SlackNet;
using SlackNet.Events;

namespace Gambot.IO.Slack
{
    public class OnSlackMessageEventArgs
    {
        public MessageEvent Event { get; set; }
    }

    public delegate Task OnSlackMessageEventHandler(object sender, OnSlackMessageEventArgs e);

    public class SlackMessageHandler : IEventHandler<MessageEvent>
    {
        public OnSlackMessageEventHandler OnSlackMessage;

        public async Task Handle(MessageEvent slackEvent)
        {
            await OnSlackMessage?.Invoke(this, new OnSlackMessageEventArgs { Event = slackEvent });
        }
    }
}