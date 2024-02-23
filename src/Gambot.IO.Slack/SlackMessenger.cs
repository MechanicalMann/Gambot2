using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using SlackNet;
using SlackNet.Events;
using ILogger = Gambot.Core.ILogger;

namespace Gambot.IO.Slack
{
    public class SlackMessenger : IMessenger, IPersonProvider
    {
        private readonly ILogger _log;
        private readonly SlackConfiguration _config;
        private readonly ConcurrentDictionary<string, SlackPerson> _personCache = new ConcurrentDictionary<string, SlackPerson>();
        private Regex _botMention;
        private ISlackApiClient _apiClient;
        private ISlackSocketModeClient _socketClient;
        private string _userId;

        public SlackMessenger(SlackConfiguration config, ILogger log)
        {
            _config = config;
            _log = log;
        }

        public event OnMessageReceivedEventHandler OnMessageReceived;

        public async Task<bool> Connect()
        {
            _log.Debug("Connecting to Slack...");
            try
            {
                var serviceBuilder = new SlackServiceBuilder()
                    .UseApiToken(_config.ApiToken)
                    .UseAppLevelToken(_config.AppLevelToken)
                    .RegisterEventHandler(ctx =>
                    {
                        var messageHandler = new SlackMessageHandler();
                        messageHandler.OnSlackMessage += HandleMessage;
                        return messageHandler;
                    })
                    .RegisterEventHandler((ctx) =>
                    {
                        var profileChanged = new SlackProfileChangedHandler();
                        profileChanged.OnProfileChanged += HandleProfileChanged;
                        return profileChanged;
                    });
                _socketClient = serviceBuilder.GetSocketModeClient();
                _apiClient = serviceBuilder.GetApiClient();
                await _socketClient.Connect();
                _log.Debug("Got a connection, testing authorizations and getting bot user ID.");
                var identity = await _apiClient.Auth.Test();
                _userId = identity.UserId;
                _botMention = new Regex($"<@{_userId}>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unable to connect to Slack.");
                return false;
            }
            try
            {
                _log.Debug("Caching user info.");
                await InitUserCache();
                _log.Debug("User info cached.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unable to populate user info cache.");
                throw;
            }
            _log.Info("Connected to Slack!");
            return true;
        }

        public Task Disconnect()
        {
            _socketClient?.Disconnect();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _socketClient?.Dispose();
            _socketClient = null;
        }

        private async Task HandleMessage(object o, OnSlackMessageEventArgs e)
        {
            if (OnMessageReceived == null)
                return;
            if (e.Event.User == _userId)
                return;
            _log.Trace($"Got message: ({e.Event.Type}:{e.Event.Subtype} in team {e.Event.Team} #{e.Event.Channel} [{e.Event.ChannelType}]) ID: {e.Event.ClientMsgId}");
            var message = await GetGambotMessage(e.Event);
            await OnMessageReceived.Invoke(this, new OnMessageReceivedEventArgs { Message = message });
        }

        private async Task InitUserCache()
        {
            _log.Trace("Initializing user cache.");
            _personCache.Clear();

            string cursor = null;
            while (true)
            {
                var result = await _apiClient.Users.List(cursor, limit: 50);

                _log.Debug($"Found {result.Members.Count} members in the Slack, caching...");
                foreach (var m in result.Members)
                {
                    if (m.Deleted)
                    {
                        _log.Trace($"Skipping deleted user {m.Id}");
                        continue;
                    }
                    if (!_personCache.TryAdd(m.Id, new SlackPerson(m)))
                    {
                        _log.Warn($"Found duplicate user when populating user cache: {m.Id}");
                    }
                }

                cursor = result.ResponseMetadata?.NextCursor;
                if (String.IsNullOrEmpty(cursor))
                {
                    break;
                }
            }
        }

        public Task HandleProfileChanged(object o, OnProfileChangedEventArgs e)
        {
            _log.Trace($"Got updated profile data for user {e.Event.User.Id}");
            var person = new SlackPerson(e.Event.User);
            _personCache.AddOrUpdate(person.Id, person, (_, __) => person);
            _log.Trace($"User data cache updated");
            return Task.CompletedTask;
        }

        internal async Task<SlackPerson> GetCachedPerson(string id)
        {
            if (_personCache.TryGetValue(id, out var person))
            {
                _log.Trace($"Got cached profile info for user {id}");
                return person;
            }
            _log.Warn($"User data cache miss: unknown person with ID {id}");
            var user = await _apiClient.Users.Info(id);
            if (user == null)
            {
                _log.Error($"Slack returned no data for user with ID {id}");
                return null;
            }
            person = new SlackPerson(user);
            _personCache.AddOrUpdate(person.Id, person, (_, __) => person);
            return person;
        }

        public async Task<IEnumerable<Person>> GetActiveUsers(string channel)
        {
            var response = await _apiClient.Conversations.Members(channel);
            var presences = await Task.WhenAll(response.Members.Select(async m =>
            {
                var p = await _apiClient.Users.GetPresence(m);
                return new { id = m, presence = p };
            }));
            return await Task.WhenAll(presences
                .Where(p => p.presence == Presence.Active)
                .Select(async p => (await GetCachedPerson(p.id)).WithPresence(Presence.Active))
            );
        }

        public async Task<IEnumerable<Message>> GetMessageHistory(string channel, string user = null)
        {
            _log.Warn("Getting history is expensive right now! Build a user info cache!");
            var history = await _apiClient.Conversations.History(channel, limit: 200);
            return await Task.WhenAll(history.Messages.Select(async m => await GetGambotMessage(m)));
        }

        public async Task<Person> GetPerson(string channel, string id)
        {
            var person = GetCachedPerson(id);
            var presence = _apiClient.Users.GetPresence(id);
            await Task.WhenAll(person, presence);
            return person.Result.WithPresence(presence.Result);
        }

        public async Task<Person> GetPersonByName(string channel, string name)
        {
            var person = _personCache.Values.FirstOrDefault(p =>
                String.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)
                || String.Equals(p.Mention, name, StringComparison.OrdinalIgnoreCase));
            var presence = await _apiClient.Users.GetPresence(person.Id);
            return person.WithPresence(presence);
        }

        public async Task SendMessage(string channel, string message, bool action)
        {
            if (action)
            {
                await _apiClient.Chat.MeMessage(channel, message);
            }
            else
            {
                await _apiClient.Chat.PostMessage(new SlackNet.WebApi.Message
                {
                    Text = message,
                    Channel = channel,
                });
            }
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

            var person = await GetCachedPerson(e.User);
            return new Message(addressed, direct, action, text.Trim(), e.Channel, person, to, this);
        }
    }
}