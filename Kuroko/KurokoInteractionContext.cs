using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.Database;
using Kuroko.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko;

public class KurokoInteractionContext(
    DiscordShardedClient client,
    SocketInteraction interaction,
    IServiceProvider serviceProvider)
    : ShardedInteractionContext(client, interaction)
{
    private KurokoConfig _config;
    private DatabaseContext _db;

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public KurokoConfig KurokoConfig => _config ??= ServiceProvider.GetRequiredService<KurokoConfig>();
    
    public DatabaseContext Database => _db ??= ServiceProvider.GetRequiredService<DatabaseContext>();
}