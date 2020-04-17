using System.Threading.Tasks;

namespace Gambot.Data
{
    public interface IDataStoreProvider
    {
        Task<IDataStore> GetDataStore(string key);
    }
}