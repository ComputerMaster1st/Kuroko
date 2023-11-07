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
    public class ReportedMsgDeletedEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;

        public ReportedMsgDeletedEvent(DiscordShardedClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            _client.MessageDeleted += (m, c) => Task.Factory.StartNew(() => MessageDeletedAsync(m));
        }

        private async Task MessageDeletedAsync(Cacheable<IMessage, ulong> oldMsg)
        {
            using var db = _services.GetRequiredService<DatabaseContext>();

            var ticket = await db.Tickets.FirstOrDefaultAsync(x => x.ReportedMessageId == oldMsg.Id);

            if (ticket is null)
                return;

            var msgEntity = await db.Messages.FirstOrDefaultAsync(x => x.Id == oldMsg.Id);
            string msgContent = "_(! No content found !)_";
            DateTimeOffset? dateTimeOffset = DateTimeOffset.UtcNow;

            if (msgEntity != null)
            {
                if (msgEntity.EditedMessages.Count > 0)
                {
                    var lastMsg = msgEntity.EditedMessages.LastOrDefault();

                    msgContent = lastMsg.Content;
                    dateTimeOffset = lastMsg.EditedAt;
                }
                else
                {
                    msgContent = msgEntity.Content;
                    dateTimeOffset = msgEntity.CreatedAt;
                }
            }

            var guild = _client.GetGuild(ticket.GuildId);
            var channel = guild.GetTextChannel(ticket.ChannelId);
            var embed = TrackedMessageEmbed.Build(msgContent, dateTimeOffset, "Reported Message Deleted!");

            await channel.SendMessageAsync(embed: embed);
        }
    }
}
