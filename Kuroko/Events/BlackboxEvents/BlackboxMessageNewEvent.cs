using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.BlackboxEvents
{
    [PreInitialize, KurokoEvent]
    public class BlackboxMessageNewEvent
    {
        private readonly IServiceProvider _services;
        private readonly DiscordShardedClient _client;
        private readonly BlackboxService _blackbox;
        private readonly TicketService _tickets;

        public BlackboxMessageNewEvent(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordShardedClient>();
            _blackbox = services.GetRequiredService<BlackboxService>();
            _tickets = services.GetRequiredService<TicketService>();
            _services = services;

            _client.MessageReceived += (m) => Task.Factory.StartNew(() => MessageReceivedAsync(m));
        }

        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            var msg = arg as IUserMessage;

            if (msg.Author.Id == _client.CurrentUser.Id)
                return;
            
            var channel = msg.Channel as IGuildChannel;

            GuildEntity root;
            ModLogEntity properties;
            using (var db = _services.GetRequiredService<DatabaseContext>())
            {
                properties = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == channel.GuildId);

                if (properties is null)
                    return;
                
                root = await db.Guilds.GetDataAsync(channel.GuildId);

                if (root.Tickets.Count > 0)
                {
                    foreach (var ticket in root.Tickets)
                    {
                        if (ticket.ChannelId == channel.Id)
                        {
                            await _tickets.StoreTicketMessageAsync(msg, ticket.Id, properties.SaveAttachments);
                            return;
                        }
                    }
                }
            }

            if (!properties.EnableBlackbox)
                return;

            await _blackbox.StoreMessageAsync(msg, properties.SaveAttachments, false);
        }
    }
}
