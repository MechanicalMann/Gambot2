using System.Collections.Generic;

namespace Gambot.Core
{
    public class Message
    {
        public IMessenger Messenger { get; }
        public bool Addressed { get; }
        public bool Direct { get; }
        public bool Action { get; }
        public string Text { get; }
        public string Channel { get; }
        public Person From { get; }
        public string To { get; }
        public IDictionary<string, string> Variables { get; }

        public Message(string text, IMessenger messenger)
        {
            Text = text;
            Messenger = messenger;
            Variables = new Dictionary<string, string>();
        }

        public Message(bool addressed, bool direct, bool action, string text, string channel, Person from, string to, IMessenger messenger)
        {
            Addressed = addressed;
            Direct = direct;
            Action = action;
            Text = text;
            From = from;
            Channel = channel;
            To = to;
            Messenger = messenger;
            Variables = new Dictionary<string, string>();
        }

        public Response Respond(string text, bool action = false)
        {
            return new Response(this, text, action);
        }
    }
}