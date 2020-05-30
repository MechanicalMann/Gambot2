using System.Threading.Tasks;

namespace Gambot.Data
{
    public interface IDataDumper
    {
        Task<string> Dump(string dataStore, string key);
    }
}