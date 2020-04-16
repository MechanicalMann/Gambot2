using System.Threading.Tasks;

namespace Gambot.Core
{
    public abstract class GambotModule
    {
        public abstract string Name { get; }

        protected ILogger Log { get; set; }

        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }
    }
}