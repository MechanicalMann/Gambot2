using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Inventory
{
    public class InventoryVariableHandler : IVariableHandler
    {
        private Regex _varRegex = new Regex("(?:(item)|(giveitem)|(newitem))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private IDataStoreProvider _dataStoreProvider;

        public InventoryVariableHandler(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<string> GetValue(string variable, Message context)
        {
            var match = _varRegex.Match(variable);
            if (!match.Success)
                return null;

            var inventory = await _dataStoreProvider.GetDataStore("Inventory");

            // $item returns an item from the inventory
            if (match.Groups[1].Success)
            {
                var item = await inventory.GetRandom("CurrentInventory");
                return item?.Value ?? "bananas";
            }
            // $giveitem returns an item from the inventory and discards it
            if (match.Groups[2].Success)
            {
                var giveitem = await inventory.GetRandom("CurrentInventory");
                if (giveitem == null)
                    return "bananas";
                await inventory.Remove(giveitem.Id);
                return giveitem.Value;
            }
            // $newitem returns some historical item
            if (match.Groups[3].Success)
            {
                var newitem = await inventory.GetRandom("History");
                return newitem?.Value ?? "bananas";
            }

            return null;
        }
    }
}