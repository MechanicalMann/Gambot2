using System.Threading.Tasks;

namespace Gambot.Core
{
    public interface IListener : IMessageHandler
    {
        Task Listen(Message message);
    }
}