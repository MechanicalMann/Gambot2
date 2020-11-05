using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.Inventory
{
    public class ListInventoryCommand : ICommand
    {
        private readonly Regex _commandRegex = new Regex("^list inventory$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private IDataStoreProvider _dataStoreProvider;

        public ListInventoryCommand(IDataStoreProvider dataStoreProvider)
        {
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<Response> Handle(Message message)
        {
            var match = _commandRegex.Match(message.Text);
            if (!match.Success)
                return null;

            var inventory = await _dataStoreProvider.GetDataStore("Inventory");
            var items = (await inventory.GetAll("CurrentInventory"))
                .Select(item => item.Value)
                .ToList();
            if (items.Count == 0)
                return message.Respond("I have nothing.");
            if (items.Count == 1)
                return message.Respond($"I have {items.Single()}.");
            if (items.Count == 2)
                return message.Respond($"I have {items.First()} and {items.Last()}.");

            return message.Respond(
                $"I have {string.Join(", ", items.Take(items.Count - 1))}, and {items.Last()}."
            );
        }
    }
}