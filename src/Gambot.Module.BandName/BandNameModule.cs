using System.Linq;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.BandName
{
    public class BandNameModule : GambotModule
    {
        public override string Name => "BandName";

        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly IConfig _config;

        public BandNameModule(IDataStoreProvider dataStoreProvider, IConfig config)
        {
            _dataStoreProvider = dataStoreProvider;
            _config = config;
        }

        public override async Task Initialize()
        {
            var bandNameChance = await _config.Get("PercentChanceOfBandName");
            if (bandNameChance == null)
                await _config.Set("PercentChanceOfBandName", "5");

            var factoids = await _dataStoreProvider.GetDataStore("Factoids");
            var values = await factoids.GetAll("band name reply");
            if (!values.Any())
                await factoids.Add("band name reply", "<reply> \"$band\" would be a cool name for a band.");
        }
    }
}