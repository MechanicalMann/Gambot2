using System;
using System.Threading.Tasks;
using Gambot.Core;

namespace Gambot.Module.Chain
{
    public class ChainResponder : IResponder
    {
        private readonly IConfig _config;
        private readonly Random _random = new Random();

        public ChainResponder(IConfig config)
        {
            _config = config;
        }

        public async Task<Response> Respond(Message message)
        {
            var chain = ChainStore.TrackMessage(message.Channel, message.Text);

            var shouldParticipate = await ShouldParticipateIn(chain);
            if (!shouldParticipate)
                return null;

            ChainStore.ResetChain(message.Channel);
            return message.Respond(message.Text, message.Action);
        }

        private async Task<bool> ShouldParticipateIn(MessageChain chain)
        {
            var baseChance = Double.Parse(await _config.Get("MessageChainBaseChance", "0.125"));
            var multiplier = Double.Parse(await _config.Get("MessageChainMultiplier", "2"));

            var chanceOfParticipation = 0.0;

            if (chain.Length > 1)
                chanceOfParticipation = baseChance * Math.Pow(multiplier, chain.Length - 2);

            return _random.NextDouble() < chanceOfParticipation;
        }
    }
}