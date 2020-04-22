using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.People
{
    public class PeopleModule : GambotModule
    {
        private Dictionary<string, string> _seedData = new Dictionary<string, string>
        { { "they", "they;them;their;theirs;themself" },
            { "one", "one;one;one's;one's;oneself" },
            { "she", "she;her;her;hers;herself" },
            { "he", "he;him;his;his;himself" },
            { "it", "it;it;its;its;itself" },
            { "e", "e;em;es;es;emself" },
            { "ey", "ey;em;eir;eirs;eirself" },
            { "hu", "hu;hum;hus;hus;humself" },
            { "peh", "peh;pehm;peh's;peh's;pehself" },
            { "per", "per;per;per;pers;perself" },
            { "sie", "sie;hir;hir;hirs;hirself" },
            { "thon", "thon;thon;thons;thons;thonself" },
            { "ve", "ve;ver;vis;vis;verself" },
            { "xe", "xe;xem;xyr;xyrs;xemself" },
            { "yo", "yo;yo;yo's;yo's;yosself" },
            { "ze/hir", "ze;hir;hir;hirs;hirself" },
            { "ze/mer", "ze;mer;zer;zers;zemself" },
            { "ze/zem", "ze;zem;zes;zes;zemself" },
            { "ze/zir", "ze;zir;zirs;zirself" },
            { "zhe", "zhe;zhim;zher;zhers;zhimself" },
        };

        public override string Name => "People";

        private readonly IDataStoreProvider _dataStoreProvider;

        public PeopleModule(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public override async Task Initialize()
        {
            var dataStore = await _dataStoreProvider.GetDataStore("Pronouns");
            var keys = await dataStore.GetAllKeys();
            if (keys.Any())
                return;

            // Seed data
            await Task.WhenAll(_seedData.Select(kvp => dataStore.Add(kvp.Key, kvp.Value)));
        }
    }
}