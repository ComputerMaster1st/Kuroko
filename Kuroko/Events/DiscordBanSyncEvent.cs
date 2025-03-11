using Discord.WebSocket;
using Kuroko.Attributes;

namespace Kuroko.Events;

[PreInitialize, KurokoEvent]
public class DiscordBanSyncEvent
{
    private readonly IServiceProvider _services;
    
    public DiscordBanSyncEvent(DiscordShardedClient client, IServiceProvider services)
    {
        _services = services;
        
        client.UserBanned += ClientOnUserBannedAsync;
    }

    private async Task ClientOnUserBannedAsync(SocketUser user, SocketGuild guild)
    {
        throw new NotImplementedException();
        
        // Get current guild bansync profile
        // Loop through host for
        // loop through client as
    }
}