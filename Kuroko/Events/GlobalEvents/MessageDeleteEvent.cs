using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.GlobalEvents
{
    [PreInitialize]
    public class MessageDeleteEvent
    {
        private readonly IServiceProvider _services;

        public MessageDeleteEvent(DiscordShardedClient client, IServiceProvider services)
        {
            _services = services;

            client.MessageDeleted += (msg, channel) => Task.Factory.StartNew(() => MessageDeleted(msg));
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> deletedMsg)
        {
            using var db = _services.GetRequiredService<DatabaseContext>();

            var msgEntity = await db.Messages.FirstOrDefaultAsync(x => x.Id == deletedMsg.Id);

            if (msgEntity is null)
                return;

            msgEntity.DeletedAt = DateTime.UtcNow;
        }
    }
}
