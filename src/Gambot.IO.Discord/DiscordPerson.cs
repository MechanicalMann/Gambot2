using System.Linq;
using Discord;
using Discord.WebSocket;
using Gambot.Core;

namespace Gambot.IO.Discord
{
    public class DiscordPerson : Person
    {
        public string Username { get; set; }
        public string Discriminator { get; set; }

        public DiscordPerson(IUser user)
        {
            Id = user.Id.ToString();
            Name = user.Username;
            Mention = user.Mention;
            Username = user.Username;
            Discriminator = user.Discriminator;

            IsActive = user.Status == UserStatus.Online || user.Status == UserStatus.Idle;
            
            var su = user as SocketGuildUser;
            if (su != null)
            {
                if (su.Nickname != null)
                    Name = su.Nickname;
                IsAdmin = su.Roles.Any(x => x.Permissions.Administrator || x.Permissions.ManageGuild);
            }
        }
    }
}