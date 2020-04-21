using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Gambot.Core;

namespace Gambot.IO.Discord
{
    public class DiscordMessenger : IMessenger
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

        public event EventHandler<OnMessageReceivedEventArgs> OnMessageReceived;

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

        private Task ReceiveMessage(SocketMessage message)
        {
            if (OnMessageReceived == null)
                return Task.CompletedTask;
            if (message.Author.Id == _client.CurrentUser.Id)
                return Task.CompletedTask;

            var addressed = false;
            var text = message.Content;

            if (message.Content.StartsWith("gambot, ", StringComparison.OrdinalIgnoreCase))
            {
                addressed = true;
                text = text.Substring(8);
            }
            if (message.MentionedUsers.Any(x => x.Id == _client.CurrentUser.Id))
            {
                addressed = true;
                text = text.Replace(_client.CurrentUser.Mention, "").Trim();
            }

            string to = null;
            var tagged = message.MentionedUsers.FirstOrDefault(u => u.Id != _client.CurrentUser.Id);
            if (tagged != null)
            {
                to = tagged.Mention;
            }
            else
            {
                var match = Regex.Match(text, @"^(.+):.*$");
                if (match.Success)
                {
                    to = match.Groups[1].Value;
                    text = text.Substring(match.Groups[1].Length);
                }
            }

            var eventArgs = new OnMessageReceivedEventArgs
            {
                Message = new Message(addressed, false, false, text, message.Channel.Id.ToString(), message.Author.Mention, to, this)
            };
            OnMessageReceived.Invoke(this, eventArgs);
            return Task.CompletedTask;
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