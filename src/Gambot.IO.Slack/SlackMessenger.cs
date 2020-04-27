using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gambot.Core;
using SlackConnector;
using SlackConnector.Models;

namespace Gambot.IO.Slack
{
    public class SlackMessenger : IMessenger, IPersonProvider
    {
        private readonly ILogger _log;
        private readonly SlackConfiguration _config;

        private readonly ConcurrentDictionary<string, ConcurrentQueue<Message>> _history = new ConcurrentDictionary<string, ConcurrentQueue<Message>>();
        private readonly ConcurrentDictionary<string, DateTime> _lastSeen = new ConcurrentDictionary<string, DateTime>();

        private ISlackConnection _connection;

        public SlackMessenger(SlackConfiguration config, ILogger log)
        {
            _config = config;
            _log = log;
        }

        public event OnMessageReceivedEventHandler OnMessageReceived;

        public async Task<bool> Connect()
        {
            _log.Debug("Connecting to Slack.");
            var connector = new SlackConnector.SlackConnector();
            _connection = await connector.Connect(_config.Token);
            _connection.OnMessageReceived += ReceiveMessage;
            _connection.OnDisconnect += HandleDisconnect;
            return true;
        }

        public async Task Disconnect()
        {
            _connection.OnDisconnect -= HandleDisconnect;
            await _connection.Close();
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.OnDisconnect -= HandleDisconnect;
                _connection.Close().GetAwaiter().GetResult();
                _connection = null;
            }
        }

        public async Task<IEnumerable<Message>> GetMessageHistory(string channel, string user = null)
        {
            _log.Warn("Slack message history is not currently supported by SlackConnector.");
            if (!_history.TryGetValue(channel, out var history))
                return Enumerable.Empty<Message>();
            return history.ToList();
        }

        public async Task SendMessage(string channel, string message, bool action)
        {
            if (!_connection.ConnectedHubs.TryGetValue(channel, out var target))
                return;
            await _connection.Say(new BotMessage { ChatHub = target, Text = message });
        }

        public async Task<IEnumerable<Person>> GetActiveUsers(string channel)
        {
            _log.Warn("Slack user presence is not currently supported by SlackConnector.");
            if (!_connection.ConnectedHubs.TryGetValue(channel, out var target))
                return Enumerable.Empty<Person>();
            var users = await _connection.GetUsers();
            var inChannel = users.Where(x => target.Members.Contains(x.Id));
            DateTime lastSeen, cutoff = DateTime.Now.AddMinutes(-15);
            var active = inChannel.Where(x => _lastSeen.TryGetValue(x.Id, out lastSeen) && lastSeen > cutoff);
            return active.Select(u => new Person { Name = u.Id, IsActive = true, IsAdmin = u.IsAdmin }).ToList();
        }

        private async void HandleDisconnect()
        {
            _log.Warn("Connection interrupted.  Attempting to reconnect.");
            _connection.OnDisconnect -= HandleDisconnect;
            _connection = null;
            await Connect();
        }

        private async Task ReceiveMessage(SlackMessage message)
        {
            if (OnMessageReceived == null)
                return;
            if (message.User.Id == _connection.Self.Id)
                return;
            
            var gm = GetGambotMessage(message);
            UpdateMessageHistory(gm);
            if (gm.Addressed)
                await _connection.IndicateTyping(message.ChatHub);
            var eventArgs = new OnMessageReceivedEventArgs
            {
                Message = gm,
            };
            await OnMessageReceived.Invoke(this, eventArgs);
        }

        private Message GetGambotMessage(SlackMessage message)
        {
            var addressed = false;
            var text = message.Text;
            string to = null;

            if (text.StartsWith("gambot, ", StringComparison.OrdinalIgnoreCase))
            {
                addressed = true;
                to = "Gambot";
                text = text.Substring(8);
            }
            if (message.MentionsBot)
            {
                addressed = true;
                text = text.Replace($"<@{_connection.Self.Id}>", "");
            }

            var match = Regex.Match(text, @"<@([a-zA-Z0-9]+)>");
            if (match.Success)
            {
                to = match.Value;
            }
            else
            {
                match = Regex.Match(text, @"^((?:[^:<>""]+?)|(?:[\\<]?:.+?:(?:\d+>)?))[,:]\s");
                if (match.Success)
                {
                    to = match.Groups[1].Value;
                    text = text.Substring(match.Groups[1].Length);
                }
            }

            return new Message(addressed, false, false, text.Trim(), message.ChatHub.Id, $"<@{message.User.Id}>", to, this);
        }

        private void UpdateMessageHistory(Message message)
        {
            var channelHistory = _history.GetOrAdd(message.Channel, new ConcurrentQueue<Message>());
            channelHistory.Enqueue(message);
            while (channelHistory.Count > 100)
                if (!channelHistory.TryDequeue(out var _))
                    break;
            _lastSeen.AddOrUpdate(message.From, DateTime.Now, (s, d) => DateTime.Now);
        }
    }
}