using System.Threading.Tasks;

namespace Gambot.Core
{
    public interface ITransformer : IMessageHandler
    {
        Task<Response> Transform(Response response);
    }
}