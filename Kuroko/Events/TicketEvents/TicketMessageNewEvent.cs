using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.TicketEvents
{
    [PreInitialize, KurokoEvent]
    public class TicketMessageNewEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;
        private readonly HttpClient _httpClient = new();

        public TicketMessageNewEvent(DiscordShardedClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            _client.MessageReceived += (arg) => Task.Factory.StartNew(() => MessageReceived(arg));
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            var msg = arg as IUserMessage;

            if (msg.Author.Id == _client.CurrentUser.Id)
                return;

            using var db = _services.GetRequiredService<DatabaseContext>();

            var ticket = await db.Tickets.FirstOrDefaultAsync(x => x.ChannelId == msg.Channel.Id);

            if (ticket is null)
                return;

            var channel = msg.Channel as ITextChannel;
            var root = await db.Guilds.FirstOrDefaultAsync(x => x.Id == channel.Guild.Id);

            if (root is null)
                return;

            var msgEntity = new MessageEntity(msg.Id, msg.Channel.Id, msg.Author.Id, msg.Content);
            ticket.Messages.Add(msgEntity, root);

            if (msg.Attachments.Count > 0)
            {
                foreach (var attachment in msg.Attachments)
                {
                    var bytes = await _httpClient.GetByteArrayAsync(attachment.Url ?? attachment.ProxyUrl);
                    msgEntity.Attachments.Add(new(attachment.Id, attachment.Filename, bytes));
                }
            }
        }
    }
}
