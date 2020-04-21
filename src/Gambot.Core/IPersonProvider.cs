using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gambot.Core
{
    public interface IPersonProvider
    {
        Task<IEnumerable<Person>> GetActiveUsers(string channel);
    }
}