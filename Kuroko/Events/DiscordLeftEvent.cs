using Discord.WebSocket;
using Kuroko.Attributes;
using Kuroko.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events;

[PreInitialize, KurokoEvent]
public class DiscordLeftEvent
{
    private readonly IServiceProvider _services;
    
    public DiscordLeftEvent(DiscordShardedClient client, IServiceProvider services)
    {
        _services = services;
        
        client.LeftGuild += ClientOnLeftGuild;
    }

    private async Task ClientOnLeftGuild(SocketGuild guild)
    {
        await using var database = _services.GetRequiredService<DatabaseContext>();
        var guildEntity = await database.Guilds.FirstOrDefaultAsync(x => x.Id == guild.Id);
        database.Guilds.Remove(guildEntity);
    }
}