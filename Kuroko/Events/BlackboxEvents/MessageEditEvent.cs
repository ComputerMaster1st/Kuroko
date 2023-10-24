using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.BlackboxEvents
{
    [PreInitialize, KurokoEvent]
    public class MessageEditEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;

        public MessageEditEvent(DiscordShardedClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            _client.MessageUpdated += (before, after, channel) => Task.Factory.StartNew(() => MessageUpdated(after));
        }

        private async Task MessageUpdated(SocketMessage after)
        {
            var msg = after as IUserMessage;

            if (msg.Author.Id == _client.CurrentUser.Id)
                return;

            using var db = _services.GetRequiredService<DatabaseContext>();

            var msgEntity = await db.Messages.FirstOrDefaultAsync(x => x.Id == msg.Id);

            if (msgEntity is null)
                return;

            msgEntity.EditedMessages.Add(new(msg.Content));
        }
    }
}
