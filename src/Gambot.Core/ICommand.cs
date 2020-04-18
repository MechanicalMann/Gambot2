using System.Threading.Tasks;

namespace Gambot.Core
{
    public interface ICommand : IMessageHandler
    {
        Task<Response> Handle(Message message);
    }
}