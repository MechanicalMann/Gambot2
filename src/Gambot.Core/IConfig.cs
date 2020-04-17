using System.Threading.Tasks;

namespace Gambot.Core
{
    public interface IConfig
    {
        Task<string> Get(string key, string defaultValue = null);
        Task Set(string key, string value);
    }
}