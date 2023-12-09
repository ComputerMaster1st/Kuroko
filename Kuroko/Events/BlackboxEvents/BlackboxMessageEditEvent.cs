using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Services;

namespace Kuroko.Events.BlackboxEvents
{
    [PreInitialize, KurokoEvent]
    public class BlackboxMessageEditEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly BlackboxService _blackbox;

        public BlackboxMessageEditEvent(DiscordShardedClient client, BlackboxService blackbox)
        {
            _client = client;
            _blackbox = blackbox;

            _client.MessageUpdated += (before, after, channel) => Task.Factory.StartNew(() => MessageUpdated(before, after));
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after)
        {
            var msg = after as IUserMessage;

            if (msg.Author.Id == _client.CurrentUser.Id  ||
               (before.HasValue && before.Value.Content == after.Content))
                return;
            
            await _blackbox.EditMessageAsync(msg.Id, msg.Content);
        }
    }
}
