using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Modules.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.TicketEvents
{
    [PreInitialize, KurokoEvent]
    public class ReportedMsgEditedEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;

        public ReportedMsgEditedEvent(DiscordShardedClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            client.MessageUpdated += (a, b, c) => Task.Factory.StartNew(() => MessageUpdatedAsync(a, b, c));
        }

        private async Task MessageUpdatedAsync(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel chn)
        {
            using var db = _services.GetRequiredService<DatabaseContext>();

            var ticket = await db.Tickets.FirstOrDefaultAsync(x => x.ReportedMessageId == after.Id);

            if (ticket is null)
                return;

            var guild = _client.GetGuild(ticket.GuildId);
            var channel = guild.GetTextChannel(ticket.ChannelId);
            var embed = ReportedMessageBuilder.Build(after.Content, after.EditedTimestamp, "Reported Message Edited!");

            await channel.SendMessageAsync(embed: embed);
        }
    }
}
