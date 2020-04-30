using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Mongle
{
    public class MongleModule : GambotModule
    {
        // A selection of autocorrect fuckups, real and imagined
        private readonly List < (string key, string value) > _monglings = new List < (string key, string value) >
        {
            (key: "because", value: "breasts"),
            (key: "because", value: "bassist"),
            (key: "before", value: "brute"),
            (key: "before", value: "beefier"),
            (key: "big", value: "but"),
            (key: "big", value: "boof"),
            (key: "bigger", value: "burger"),
            (key: "bigger", value: "bidet"),
            (key: "boggling", value: "bottling"),
            (key: "boggling", value: "blogging"),
            (key: "burger", value: "budget"),
            (key: "burger", value: "butter"),
            (key: "close", value: "Dirac"),
            (key: "close", value: "voter"),
            (key: "coding", value: "chipotle"),
            (key: "coding", value: "voiding"),
            (key: "confusing", value: "diffusing"),
            (key: "express", value: "excess"),
            (key: "fuck", value: "duck"),
            (key: "fuck", value: "suck"),
            (key: "fucking", value: "ducking"),
            (key: "fucking", value: "puking"),
            (key: "got", value: "fur"),
            (key: "hell", value: "earth"),
            (key: "invite", value: "iCloud"),
            (key: "it", value: "tit"),
            (key: "it", value: "out"),
            (key: "it", value: "ur"),
            (key: "keeps", value: "Leroux"),
            (key: "know", value: "lincoln"),
            (key: "line", value: "lube"),
            (key: "moving", value: "movies"),
            (key: "much", value: "but"),
            (key: "much", value: "mulch"),
            (key: "people", value: "purple"),
            (key: "push", value: "peak"),
            (key: "pushing", value: "pausing"),
            (key: "reason", value: "train"),
            (key: "reason", value: "raisin"),
            (key: "someone", value: "sometimes"),
            (key: "someone", value: "donations"),
            (key: "someone", value: "sunrooms"),
            (key: "someone", value: "dungeons"),
            (key: "surely", value: "surgery"),
            (key: "swear", value: "swart"),
            (key: "sweaty", value: "dessert"),
            (key: "talk", value: "task"),
            (key: "talk", value: "teak"),
            (key: "talked", value: "tarnished"),
            (key: "talked", value: "raked"),
            (key: "to", value: "Rio"),
            (key: "to", value: "tio"),
            (key: "we", value: "why"),
            (key: "you", value: "tout"),
            (key: "you", value: "toy"),
        };

        public override string Name => "Mongle";

        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly IConfig _config;

        public MongleModule(IDataStoreProvider dataStoreProvider, IConfig config)
        {
            _dataStoreProvider = dataStoreProvider;
            _config = config;
        }

        public override async Task Initialize()
        {
            var dataStore = await _dataStoreProvider.GetDataStore("Monglings");
            var allKeys = await dataStore.GetAllKeys();
            if (allKeys.Any())
                return;

            await Task.WhenAll(
                _config.Set("PercentChanceOfMongling", "1"),
                _config.Set("PercentChanceOfMongledSwaps", "66"),
                _config.Set("PercentChanceOfMongledSpaces", "75"),
                _config.Set("PercentChanceOfMongledLetters", "99"));
            await Task.WhenAll(_monglings.Select(async pair => await dataStore.Add(pair.key, pair.value)));
        }
    }
}