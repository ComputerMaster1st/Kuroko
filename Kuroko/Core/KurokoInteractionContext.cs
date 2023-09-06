using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Core
{
    public class KurokoInteractionContext : ShardedInteractionContext
    {
        private DatabaseContext _db = null;

        public IServiceProvider ServiceProvider { get; }

        public DatabaseContext Database
        {
            get
            {
                _db ??= ServiceProvider.GetRequiredService<DatabaseContext>();
                return _db;
            }
        }

        public KurokoInteractionContext(DiscordShardedClient client, SocketInteraction interaction, IServiceProvider serviceProvider) : base(client, interaction)
            => ServiceProvider = serviceProvider;
    }
}
