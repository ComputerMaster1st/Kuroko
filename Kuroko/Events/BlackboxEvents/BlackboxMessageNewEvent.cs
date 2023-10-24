using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.BlackboxEvents
{
    [PreInitialize, KurokoEvent]
    public class BlackboxMessageNewEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;
        private readonly HttpClient _httpClient = new();

        public BlackboxMessageNewEvent(DiscordShardedClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            _client.MessageReceived += (m) => Task.Factory.StartNew(() => MessageReceivedAsync(m));
        }

        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            var msg = arg as IUserMessage;

            if (msg.Author.Id == _client.CurrentUser.Id)
                return;

            var channel = msg.Channel as IGuildChannel;
            using var db = _services.GetRequiredService<DatabaseContext>();

            var modLogEntity = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == channel.GuildId);

            if (modLogEntity is null || !modLogEntity.EnableBlackbox)
                return;

            if (db.Tickets.Any(x => x.ChannelId == msg.Channel.Id))
                return;

            var recordedMsg = new MessageEntity(msg.Id, channel.Id, msg.Author.Id, msg.Content);

            if (msg.Attachments.Count > 0)
            {
                foreach (var attachment in msg.Attachments)
                {
                    var bytes = await _httpClient.GetByteArrayAsync(attachment.Url ?? attachment.ProxyUrl);
                    recordedMsg.Attachments.Add(new AttachmentEntity(attachment.Id, attachment.Filename, bytes));
                }
            }

            var root = await db.Guilds.GetOrCreateRootAsync(channel.GuildId);
            root.Messages.Add(recordedMsg);
        }
    }
}
