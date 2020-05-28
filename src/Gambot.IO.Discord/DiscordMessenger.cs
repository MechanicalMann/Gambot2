using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Gambot.Core;

namespace Gambot.IO.Discord
{
    public class DiscordMessenger : IMessenger, IPersonProvider
    {
        private readonly ILogger _log;
        private readonly LogSeverity _logSeverity;
        private readonly string _botToken;
        private DiscordSocketClient _client;

        public DiscordMessenger(DiscordConfiguration config, ILogger log)
        {
            _log = log;
            _botToken = config.Token;
            _logSeverity = (LogSeverity) config.LogSeverity;
        }

        public event OnMessageReceivedEventHandler OnMessageReceived;

        public async Task<bool> Connect()
        {
            _log.Debug("Connecting to Discord.");
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = _logSeverity,

            });
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, _botToken);
            await _client.StartAsync();
            _client.MessageReceived += ReceiveMessage;
            return true;
        }

        public Task Disconnect()
        {
            return _client.StopAsync();
        }

        public void Dispose()
        {
            if (_client != null)
                _client.Dispose();
        }

        public async Task SendMessage(string channel, string message, bool action)
        {
            var target = _client.GetChannel(UInt64.Parse(channel)) as ISocketMessageChannel;
            if (target == null)
                return;
            await target.SendMessageAsync(message);
        }

        public async Task<IEnumerable<Message>> GetMessageHistory(string channel, string user = null)
        {
            var target = _client.GetChannel(UInt64.Parse(channel)) as ISocketMessageChannel;
            if (target == null)
                return Enumerable.Empty<Message>();
            var history = (await target.GetMessagesAsync(100).FlattenAsync());
            if (user != null)
            {
                history = history.Where(m => m.Author.Mention == user || m.Author.Username == user || (m.Author as SocketGuildUser)?.Nickname == user);
            }
            return history.Reverse().Select(m => GetGambotMessage(m));
        }

        public async Task<IEnumerable<Person>> GetActiveUsers(string channel)
        {
            var target = _client.GetChannel(UInt64.Parse(channel)) as ISocketMessageChannel;
            if (target == null)
                return Enumerable.Empty<Person>();
            var users = await target.GetUsersAsync().FlattenAsync();
            return users.Where(x => x.Id != _client.CurrentUser.Id).Select(x => new DiscordPerson(x)).Where(x => x.IsActive);
        }

        public async Task<Person> GetPerson(string channel, string id)
        {
            var target = _client.GetChannel(UInt64.Parse(channel)) as ISocketMessageChannel;
            if (target == null)
                return null;
            var user = await target.GetUserAsync(UInt64.Parse(id));
            if (user == null)
                return null;
            return new DiscordPerson(user);
        }

        public async Task<Person> GetPersonByName(string channel, string name)
        {
            var target = _client.GetChannel(UInt64.Parse(channel)) as ISocketMessageChannel;
            if (target == null)
                return null;
            var users = await target.GetUsersAsync().FlattenAsync();
            return users.Select(x => new DiscordPerson(x)).FirstOrDefault(p => p.Mention == name || p.Name == name || p.Username == p.Username);
        }

        private async Task ReceiveMessage(SocketMessage message)
        {
            if (OnMessageReceived == null)
                return;
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            var gm = GetGambotMessage(message);
            var typing = gm.Addressed ? message.Channel.EnterTypingState() : null;
            var eventArgs = new OnMessageReceivedEventArgs
            {
                Message = gm,
            };
            await OnMessageReceived.Invoke(this, eventArgs);
            typing?.Dispose();
        }

        private Message GetGambotMessage(IMessage message)
        {
            var socketMessage = message as SocketMessage;
            var direct = (message.Channel as SocketDMChannel) != null;
            var addressed = false;
            var text = message.Content;
            string to = null;

            if (direct)
            {
                addressed = true;
            }
            if (message.Content.StartsWith("gambot, ", StringComparison.OrdinalIgnoreCase))
            {
                addressed = true;
                text = text.Substring(8);
            }
            if (socketMessage?.MentionedUsers.Any(x => x.Id == _client.CurrentUser.Id) ?? false)
            {
                addressed = true;
                to = _client.CurrentUser.Mention;
                text = text.Replace(_client.CurrentUser.Mention, "");
            }

            if (addressed)
            {
                to = "Gambot";
            }
            else
            {
                var tagged = socketMessage?.MentionedUsers.FirstOrDefault(u => u.Id != _client.CurrentUser.Id);
                if (tagged != null)
                {
                    to = tagged.Mention;
                }
                else
                {
                    var match = Regex.Match(text, @"^((?:[^:<>""]+?)|(?:[\\<]?:.+?:(?:\d+>)?))[,:]\s");
                    if (match.Success)
                    {
                        to = match.Groups[1].Value;
                        text = text.Substring(match.Groups[1].Length);
                    }
                }
            }

            // Discord doesn't have a separate message type for /me messages
            // since it never tried to be IRC. But there is a /me command that
            // italicizes your entire message using underscores, which is
            // interesting, because if you use ctrl+I to italicize it uses
            // asterisks. So we can make a mildly educated guess here about
            // whether the message is likely a /me message by testing whether
            // the whole thing is surrounded in single underscores. There will
            // undoubtedly be false positives for anyone using Markdown manually
            // or people who learned Slack's "markdown" first, but this whole
            // /me message concept has always been a bit fly-by-night anyway.
            var guess = Regex.Match(text, @"^_[^_]+_$");

            return new Message(addressed, direct, guess.Success, text.Trim(), message.Channel.Id.ToString(), new DiscordPerson(message.Author), to, this);
        }

        private Task Log(LogMessage logMessage)
        {
            switch (logMessage.Severity)
            {
                case LogSeverity.Critical:
                    _log.Fatal(logMessage.Exception, logMessage.Message);
                    break;
                case LogSeverity.Error:
                    _log.Error(logMessage.Exception, logMessage.Message);
                    break;
                case LogSeverity.Warning:
                    _log.Warn(logMessage.Exception, logMessage.Message);
                    break;
                case LogSeverity.Verbose:
                    _log.Debug(logMessage.Exception, logMessage.Message);
                    break;
                case LogSeverity.Debug:
                    _log.Trace(logMessage.Exception, logMessage.Message);
                    break;
                default:
                    _log.Info(logMessage.Exception, logMessage.Message);
                    break;
            }
            return Task.CompletedTask;
        }
    }
}