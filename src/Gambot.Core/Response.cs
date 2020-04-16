using System.Threading.Tasks;

namespace Gambot.Core
{
    public class Response
    {
        public Message Message { get; }

        public bool Action { get; set; }
        public string Text { get; set; }

        public Response(Message message, string text, bool action = false)
        {
            Message = message;
            Text = text;
            Action = action;
        }

        public async Task Send()
        {
            await Message.Messenger.SendMessage(Message.Channel, Text, Action);
        }
    }
}