using System;
using System.Threading.Tasks;
namespace Gambot.Core
{
    public interface IVariableHandler
    {
        Task<string> GetValue(string variable, Message context);
    }
}