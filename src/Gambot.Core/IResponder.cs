using System.Threading.Tasks;

namespace Gambot.Core
{
    public interface IResponder : IMessageHandler
    {
        Task<Response> Respond(Message message);
    }
}