using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko;

public class KurokoInteractionContext(
    DiscordShardedClient client,
    SocketInteraction interaction,
    IServiceProvider serviceProvider)
    : ShardedInteractionContext(client, interaction)
{
    //private DatabaseContext _db = null;

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    // public DatabaseContext Database
    // {
    //     get
    //     {
    //         _db ??= ServiceProvider.GetRequiredService<DatabaseContext>();
    //         return _db;
    //     }
    // }
}