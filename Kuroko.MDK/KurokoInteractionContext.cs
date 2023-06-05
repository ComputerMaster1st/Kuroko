using Discord.Interactions;
using Discord.WebSocket;

namespace Kuroko.MDK
{
    public class KurokoInteractionContext : ShardedInteractionContext
    {
        public IServiceProvider ServiceProvider { get; }

        public KurokoInteractionContext(DiscordShardedClient client, SocketInteraction interaction, IServiceProvider serviceProvider) : base(client, interaction)
            => ServiceProvider = serviceProvider;
    }
}
