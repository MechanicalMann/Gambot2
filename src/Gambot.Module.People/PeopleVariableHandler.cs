using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Module.People
{
    public class PeopleVariableHandler : IVariableHandler
    {
        private readonly IPersonProvider _personProvider;
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly Random _random = new Random();

        public PeopleVariableHandler(IPersonProvider personProvider, IDataStoreProvider dataStoreProvider)
        {
            _personProvider = personProvider;
            _dataStoreProvider = dataStoreProvider;
        }

        protected async Task<string> GetSomeone(string channel)
        {
            var activeUsers = (await _personProvider.GetActiveUsers(channel)).ToList();
            return activeUsers.Any() ?
                activeUsers.ElementAt(_random.Next(0, activeUsers.Count)).Name :
                null;
        }

        public async Task<string> GetValue(string variable, Message context)
        {
            var match = Regex.Match(variable, @"(?:(who)|(to)|(someone))", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            var dataStore = await _dataStoreProvider.GetDataStore("PersonHistory");

            if (match.Groups[1].Success)
            {
                await dataStore.SetSingle(context.Channel, context.From);
                return context.From;
            }

            if (match.Groups[2].Success)
            {
                var to = context.To ?? await GetSomeone(context.Channel) ?? context.From;
                await dataStore.SetSingle(context.Channel, to);
                return to;
            }

            if (match.Groups[3].Success)
            {
                var someone = await GetSomeone(context.Channel) ?? context.From;
                await dataStore.SetSingle(context.Channel, someone);
                return someone;
            }

            return null;
        }
    }
}