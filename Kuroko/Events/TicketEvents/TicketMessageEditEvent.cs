using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.TicketEvents
{
    [PreInitialize]
    public class TicketMessageEditEvent
    {
        private readonly IServiceProvider _services;

        public TicketMessageEditEvent(DiscordShardedClient client, IServiceProvider services)
        {
            _services = services;

            client.MessageUpdated += (before, after, channel) => Task.Factory.StartNew(() => MessageUpdated(after));
        }

        private async Task MessageUpdated(SocketMessage after)
        {
            var msg = after as IUserMessage;

            using var db = _services.GetRequiredService<DatabaseContext>();

            var msgEntity = await db.Messages.FirstOrDefaultAsync(x => x.Id == msg.Id);

            if (msgEntity is null)
                return;

            msgEntity.EditedMessages.Add(new(msg.Content));
        }
    }
}
