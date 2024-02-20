using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using SlackNet;
using SlackNet.Events;
using ILogger = Gambot.Core.ILogger;

namespace Gambot.IO.Slack
{
    public class SlackMessageHandler : IEventHandler<MessageEvent>
    {
        private readonly string _userId;
        private readonly ILogger _log;
        private readonly ISlackApiClient _client;
        private readonly IMessenger _messenger;
        private readonly Regex _botMention;

        public OnMessageReceivedEventHandler OnMessageReceived;

        public SlackMessageHandler(ISlackApiClient client, ILogger log, IMessenger messenger, string userId)
        {
            _client = client;
            _log = log;
            _messenger = messenger;
            _userId = userId;
            _botMention = new Regex($"<@{userId}>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public async Task Handle(MessageEvent slackEvent)
        {
            if (OnMessageReceived == null)
                return;
            if (slackEvent.User == _userId)
                return;
            _log.Trace($"Got message: ({slackEvent.Type}:{slackEvent.Subtype} in team {slackEvent.Team} #{slackEvent.Channel} [{slackEvent.ChannelType}]): {slackEvent.Text}");
            await OnMessageReceived.Invoke(this, new OnMessageReceivedEventArgs { Message = await GetGambotMessage(slackEvent) });
        }

        internal async Task<Message> GetGambotMessage(MessageEvent e)
        {
            var direct = e.ChannelType == "im";
            var action = e.Subtype == "me_message";
            var addressed = false;
            var text = e.Text;
            string to = null;

            if (direct)
            {
                addressed = true;
            }
            if (text.StartsWith("gambot, ", StringComparison.OrdinalIgnoreCase))
            {
                addressed = true;
                text = text.Substring(8);
            }
            if (_botMention.Match(text).Success)
            {
                addressed = true;
                text = text.Replace($"<@{_userId}>", "");
            }

            if (addressed)
            {
                to = "Gambot";
            }
            else
            {
                var match = Regex.Match(text, @"<@([a-zA-Z0-9]+)>");
                if (match.Success)
                {
                    to = match.Value;
                    text = text.Replace(to, "").Trim();
                }
                else
                {
                    match = Regex.Match(text, @"^((?:[^:<>""]+?)|(?:[\\<]?:.+?:(?:\d+>)?))[:]\s");
                    if (match.Success)
                    {
                        to = match.Groups[1].Value;
                        text = text.Substring(match.Groups[0].Length);
                    }
                }
            }

            var userInfo = await _client.Users.Info(e.User);
            return new Message(addressed, direct, action, text.Trim(), e.Channel, new SlackPerson(userInfo) { IsActive = true }, to, _messenger);
        }
    }
}