using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;
using Gambot.Module.Factoid;

namespace Gambot.Module.Inventory
{
    public class AddToInventoryResponder : IResponder
    {
        private readonly Regex[] _commandRegexen = new []
        {
            new Regex("^(?:I give|gives) (.+) to gambot$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex("^(?:I give|gives) gambot (.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex("^(?:take|have) (.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        };

        private IDataStoreProvider _dataStoreProvider;
        private IConfig _config;
        private FactoidResponder _factoidResponder;

        public AddToInventoryResponder(IDataStoreProvider dataStoreProvider, IConfig config, FactoidResponder factoidResponder)
        {
            _dataStoreProvider = dataStoreProvider;
            _config = config;
            _factoidResponder = factoidResponder;
        }

        public async Task<Response> Respond(Message message)
        {
            var item = GetItem(message);
            if (item == null)
                return null;

            var inventory = await _dataStoreProvider.GetDataStore("Inventory");
            var items = (await inventory.GetAll("CurrentInventory")).ToList();

            if (await inventory.Contains("CurrentInventory", item))
            {
                // already exists
                return await FactoidResponse(message, "duplicate item", "I already have $item.", ("item", item));
            }

            Response response = null;
            var limit = Int32.Parse(await _config.Get("InventoryLimit", "10"));
            if (await inventory.GetCount("CurrentInventory") >= limit)
            {
                // drop something
                var dropped = await inventory.GetRandom("CurrentInventory");
                if (dropped != null) // beware race conditions
                {
                    await inventory.Remove(dropped.Id);
                    response = await FactoidResponse(message, "drops item", "I take $newitem and drop $giveitem.", ("newitem", item), ("giveitem", dropped.Value));
                }
            }

            await inventory.Add("CurrentInventory", item);
            if (!await inventory.Contains("History", item))
                await inventory.Add("History", item);

            if (response == null)
                response = await FactoidResponse(message, "takes item", "I now have $item.", ("item", item));
            return response;
        }

        private string GetItem(Message message)
        {
            foreach (var regex in _commandRegexen)
            {
                var match = regex.Match(message.Text);
                if (match.Success)
                    return match.Groups[1].Value;
            }
            return null;
        }

        private async Task<Response> FactoidResponse(Message message, string factoid, string fallback, params (string, string)[] variables)
        {
            foreach (var variable in variables)
            {
                message.Variables.Add(variable.Item1, variable.Item2);
            }

            // Quick and dirty forwarding to the factoid responder
            var response = await _factoidResponder.Respond(new Message(factoid, message.Messenger));
            return message.Respond(response?.Text ?? fallback, response?.Action ?? false);
        }
    }
}