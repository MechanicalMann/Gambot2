using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gambot.Core;
using SlackNet;
using ILogger = Gambot.Core.ILogger;

namespace Gambot.IO.Slack
{
    public class SlackMessenger : IMessenger, IPersonProvider
    {
        private readonly ILogger _log;
        private readonly SlackConfiguration _config;
        private ISlackApiClient _apiClient;
        private ISlackSocketModeClient _socketClient;
        private SlackMessageHandler _messageHandler;
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
                        _messageHandler = new SlackMessageHandler(ctx.ServiceProvider.GetApiClient(), _log.GetChildLog(typeof(SlackMessageHandler).Name), this, _userId)
                        {
                            OnMessageReceived = OnMessageReceived
                        };
                        return _messageHandler;
                    });
                _socketClient = serviceBuilder.GetSocketModeClient();
                _apiClient = serviceBuilder.GetApiClient();
                await _socketClient.Connect();
                _log.Debug("Got a connection, testing authorizations and getting bot user ID.");
                var identity = await _apiClient.Auth.Test();
                _userId = identity.UserId;
                _log.Info("Connected to Slack!");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unable to connect to Slack.");
                return false;
            }
            return true;
        }

        public async Task Disconnect()
        {
            await _apiClient?.Users.SetPresence(RequestPresence.Away);
            _socketClient?.Disconnect();
        }

        public void Dispose()
        {
            _socketClient?.Dispose();
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
                .Select(async p =>
                {
                    var u = await _apiClient.Users.Info(p.id);
                    return new SlackPerson(u) { IsActive = true };
                })
            );
        }

        public async Task<IEnumerable<Message>> GetMessageHistory(string channel, string user = null)
        {
            _log.Warn("Getting history is expensive right now! Build a user info cache!");
            var history = await _apiClient.Conversations.History(channel, limit: 200);
            return await Task.WhenAll(history.Messages.Select(async m => await _messageHandler.GetGambotMessage(m)));
        }

        public async Task<Person> GetPerson(string channel, string id)
        {
            var user = _apiClient.Users.Info(id);
            var presence = _apiClient.Users.GetPresence(id);
            await Task.WhenAll(user, presence);
            return new SlackPerson(user.Result)
            {
                IsActive = presence.Result == Presence.Active
            };
        }

        public async Task<Person> GetPersonByName(string channel, string name)
        {
            var allUsers = _apiClient.Users.List(limit: 200); // TODO: paginate through results
            var inChannel = _apiClient.Conversations.Members(channel, limit: 200);
            await Task.WhenAll(allUsers, inChannel);
            var user = allUsers.Result.Members
                .Where(m => inChannel.Result.Members.Contains(m.Id) && (m.Name == name || m.Profile.DisplayName == name))
                .FirstOrDefault();
            if (user == null)
                return null;
            var presence = await _apiClient.Users.GetPresence(user.Id);
            return new SlackPerson(user) { IsActive = presence == Presence.Active };
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
    }
}