using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Gambot.Core;
using System.Collections.Generic;

namespace Gambot.IO
{
    public class ConsoleMessenger : IMessenger
    {
        private readonly ILogger _log;
        private readonly Queue<Message> _history = new Queue<Message>();
        private bool _connected = false;
        private Thread _inputThread = null;

        public event EventHandler<OnMessageReceivedEventArgs> OnMessageReceived = delegate {};

        public ConsoleMessenger(ILogger log)
        {
            _log = log;
        }

        public Task SendMessage(string channel, string message, bool action)
        {
            _log.Trace("Sending message");
            Console.WriteLine(action ? $"* Gambot {message}" : $"Gambot: {message}");
            return Task.CompletedTask;
        }

        public Task<bool> Connect()
        {
            _inputThread = new Thread(() =>
            {
                _log.Trace("Spinning up console listener thread.");
                while (_connected)
                {
                    Console.Write("> ");
                    var message = Console.ReadLine();
                    if (_connected && !String.IsNullOrEmpty(message) && OnMessageReceived != null)
                    {
                        _log.Trace("Received message");
                        var addressed = message.StartsWith("gambot, ", StringComparison.OrdinalIgnoreCase);
                        if (addressed) message = message.Substring(8);
                        // Make debugging locally easier
                        string to = null;
                        var match = Regex.Match(message, @"^(.+): .+$");
                        if (match.Success)
                        {
                            to = match.Groups[1].Value;
                            message = message.Substring(to.Length);
                            addressed = String.Compare("gambot", to, true) == 0;
                        }
                        var m = new Message(addressed, false, false, message, "tty", "Human", null, this);
                        _history.Enqueue(m);
                        if (_history.Count > 100) _history.Dequeue();
                        OnMessageReceived.Invoke(this, new OnMessageReceivedEventArgs
                        {
                            Message = m,
                        });
                    }
                }
            });
            _connected = true;
            _inputThread.Start();
            return Task.FromResult(true);
        }

        public Task<IEnumerable<Message>> GetMessageHistory(string channel, string user = null)
        {
            return Task.FromResult(_history.AsEnumerable());
        }

        public void Dispose()
        {
            _log.Trace("Disposing");
            if (_inputThread != null)
            {
                _connected = false;
                _inputThread.Interrupt();
            }
        }

        public Task Disconnect()
        {
            _log.Debug("Disconnecting");
            return Task.CompletedTask;
        }
    }
}