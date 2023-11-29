using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Services;

namespace Kuroko.Events.BlackboxEvents
{
    [PreInitialize, KurokoEvent]
    public class BlackboxMessageNewEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly BlackboxService _blackbox;

        public BlackboxMessageNewEvent(DiscordShardedClient client, BlackboxService blackbox)
        {
            _client = client;
            _blackbox = blackbox;

            _client.MessageReceived += (m) => Task.Factory.StartNew(() => MessageReceivedAsync(m));
        }

        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            var msg = arg as IUserMessage;

            if (msg.Author.Id == _client.CurrentUser.Id)
                return;

            await _blackbox.StoreMessageAsync(msg);
        }
    }
}
