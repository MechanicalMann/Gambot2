using System;
using System.Collections.Concurrent;

namespace Gambot.Module.Chain
{
    internal static class ChainStore
    {
        // And if you don't love me now...
        private static ConcurrentDictionary<string, MessageChain> _chains = new ConcurrentDictionary<string, MessageChain>();

        // ... You will never love me again...
        public static MessageChain TrackMessage(string channel, string message)
        {
            message = message.Trim();
            return _chains.AddOrUpdate(channel, new MessageChain { Message = message, Length = 1 }, (c, chain) =>
            {
                if (String.Compare(chain.Message, message, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    chain.Length++;
                }
                else
                {
                    chain.Message = message;
                    chain.Length = 1;
                }
                return chain;
            });
        }

        // ... I can still hear you saying...
        public static MessageChain GetChain(string channel)
        {
            if (!_chains.TryGetValue(channel, out var chain))
                return null;
            return chain;
        }

        // ... You would never break the chain...
        public static void ResetChain(string channel)
        {
            _chains.AddOrUpdate(channel, new MessageChain(), (c, m) => new MessageChain());
        }
    }
}