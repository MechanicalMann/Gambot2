using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Gambot.Core
{
    public interface IMessenger : IDisposable
    {
        event EventHandler<OnMessageReceivedEventArgs> OnMessageReceived;
        Task<bool> Connect();
        Task Disconnect();
        Task SendMessage(string channel, string message, bool action);
        Task<IEnumerable<Message>> GetMessageHistory(string channel, string user = null);
    }

    public class OnMessageReceivedEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }
}