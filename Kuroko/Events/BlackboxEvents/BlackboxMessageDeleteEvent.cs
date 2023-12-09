using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Services;

namespace Kuroko.Events.BlackboxEvents
{
    [PreInitialize, KurokoEvent]
    public class BlackboxMessageDeleteEvent
    {
        public BlackboxMessageDeleteEvent(DiscordShardedClient client, BlackboxService blackbox)
            => client.MessageDeleted += (msg, channel) => Task.Factory.StartNew(() => blackbox.DeletedMessageAsync(msg.Id));
    }
}
