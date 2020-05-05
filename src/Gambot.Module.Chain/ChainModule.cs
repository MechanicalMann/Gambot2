using System.Threading.Tasks;
using Gambot.Core;

namespace Gambot.Module.Chain
{
    public class ChainModule : GambotModule
    {
        public override string Name => "Chain";

        private readonly IConfig _config;

        public ChainModule(IConfig config)
        {
            _config = config;
        }

        public override async Task Initialize()
        {
            var baseChance = await _config.Get("MessageChainBaseChance");
            if (baseChance == null)
                await _config.Set("MessageChainBaseChance", "0.125");

            var multiplier = await _config.Get("MessageChainMultiplier");
            if (multiplier == null)
                await _config.Set("MessageChainMultiplier", "2");
        }
    }
}