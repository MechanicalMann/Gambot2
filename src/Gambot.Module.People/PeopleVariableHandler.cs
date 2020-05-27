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

        protected async Task<Person> GetSomeone(string channel)
        {
            var activeUsers = (await _personProvider.GetActiveUsers(channel)).ToList();
            return activeUsers.Any()
                ? activeUsers.ElementAt(_random.Next(0, activeUsers.Count))
                : null;
        }

        public async Task<string> GetValue(string variable, Message context)
        {
            var match = Regex.Match(variable, @"(?:(who)|(to)|(someone))", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;

            var dataStore = await _dataStoreProvider.GetDataStore("PersonHistory");

            if (match.Groups[1].Success)
            {
                await dataStore.SetSingle(context.Channel, context.From.Id);
                return context.From.Name;
            }

            if (match.Groups[2].Success)
            {
                // We need to handle this differently since the message might be
                // addressed to something or someone that isn't a known person.
                Person to = null;
                if (context.To != null)
                {
                    to = await _personProvider.GetPersonByName(context.Channel, context.To);
                    if (to == null)
                    {
                        await dataStore.SetSingle(context.Channel, context.To);
                        return context.To;
                    }
                }
                to = to ?? await GetSomeone(context.Channel) ?? context.From;
                await dataStore.SetSingle(context.Channel, to.Id);
                return to.Name;
            }

            if (match.Groups[3].Success)
            {
                var someone = await GetSomeone(context.Channel) ?? context.From;
                await dataStore.SetSingle(context.Channel, someone.Id);
                return someone.Name;
            }

            return null;
        }
    }
}