using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
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

        public BlackboxMessageNewEvent(DiscordShardedClient client, BlackboxService blackbox, IServiceProvider services)
        {
            _client = client;
            _blackbox = blackbox;
            _services = services;

            _client.MessageReceived += (m) => Task.Factory.StartNew(() => MessageReceivedAsync(m));
        }

        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            var msg = arg as IUserMessage;

            if (msg.Author.Id == _client.CurrentUser.Id)
                return;
            
            var channel = msg.Channel as IGuildChannel;
            var db = _services.GetRequiredService<DatabaseContext>();
            var modLogProperties = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == channel.GuildId);

            if (modLogProperties is null || !modLogProperties.EnableBlackbox)
                return;
            
            var root = await db.Guilds.GetDataAsync(channel.GuildId);

            foreach (var ticket in root.Tickets)
                if (ticket.ChannelId == channel.Id)
                    return;

            await _blackbox.StoreMessageAsync(msg);
        }
    }
}
