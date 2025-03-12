using System.Text;
using Discord;
using Discord.WebSocket;
using Kuroko.Attributes;
using Kuroko.Commands;
using Kuroko.Database;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events;

[PreInitialize, KurokoEvent]
public class BanSyncEventClient : BanSyncEventBase
{
    private readonly DiscordShardedClient _client;
    private readonly IServiceProvider _services;
    
    public BanSyncEventClient(DiscordShardedClient client, IServiceProvider services)
    {
        _client = client;
        _services = services;
        
        client.UserBanned += (user, guild) 
            => Task.Factory.StartNew(() => ClientOnUserBannedAsync(user, guild));
    }

    private async Task ClientOnUserBannedAsync(SocketUser clientBannedUser, SocketGuild clientGuild)
    {
        await using var database = _services.GetRequiredService<DatabaseContext>();
        var properties = await database.BanSyncProperties
            .Include(banSyncProperties => banSyncProperties.ClientOfProfiles)
            .FirstOrDefaultAsync(x => x.GuildId == clientGuild.Id);

        if (!properties.IsEnabled)
            return;
        
        var components = new ComponentBuilder()
            .WithButton("(3) !! Ban User !! (3)", $"{CommandMap.BANSYNC_BANUSER}:{clientBannedUser.Id}",
                ButtonStyle.Danger)
            .WithButton("Ignore", $"{CommandMap.BANSYNC_IGNORE}", ButtonStyle.Secondary)
            .Build();
        var clientChannel = clientGuild.GetTextChannel(properties.BanSyncChannelId);
        var banReason = await clientGuild.GetBanAsync(clientBannedUser.Id);
        var reason = $"[BanSync] {
            (string.IsNullOrEmpty(banReason.Reason) ? "No Reason Given" : $"Reason: {banReason.Reason}")}";
        var embedWarn = CreateEmbed("BanSync - Incoming Warning!", clientBannedUser.GlobalName,
            clientBannedUser.Id, $"User Banned From: {clientGuild.Name}",
            clientBannedUser.GetAvatarUrl(), reason);
        var embedBan = CreateEmbed("BanSync - Auto-Ban Performed!", clientBannedUser.GlobalName,
            clientBannedUser.Id, $"User Banned From: {clientGuild.Name}",
            clientBannedUser.GetAvatarUrl(), reason);
        var declineCount = 0;
        var warnCount = 0;
        var banCount = 0;
        var embedSync = CreateEmbed("BanSync - User Banned & Synced!", 
            clientBannedUser.GlobalName, clientBannedUser.Id, 
            new StringBuilder()
                .AppendLine($"Hosts Declined: {declineCount}")
                .AppendLine($"Warnings Sent: {warnCount}")
                .AppendLine($"Bans Executed: {banCount}")
                .ToString(),
            clientBannedUser.GetAvatarUrl(), reason);
        
        foreach (var profile in properties.ClientOfProfiles)
        {
            if (!profile.HostProperties.IsEnabled)
                return;
            
            var hostGuild = _client.GetGuild(profile.HostProperties.GuildId);
            var hostUser = hostGuild.GetUser(clientBannedUser.Id);
            var hostChannel = hostGuild.GetTextChannel(profile.HostProperties.BanSyncChannelId);

            if (await hostGuild.GetBanAsync(clientBannedUser.Id) != null)
                continue;

            switch (profile.Mode)
            {
                case BanSyncMode.Default:
                    var hostMode = profile.HostProperties.HostMode;
                    switch (hostMode)
                    {
                        case BanSyncMode.FullDuplex:
                            goto FULLDUPLEX;
                        case BanSyncMode.HalfDuplex:
                            goto HALFDUPLEX;
                        default:
                            goto DEFAULT;
                    }
                case BanSyncMode.HalfDuplex:
                    HALFDUPLEX:
                    if (hostChannel != null)
                        await hostChannel.SendMessageAsync(embed: embedWarn, components: components);
                    warnCount++;
                    break;
                case BanSyncMode.FullDuplex:
                    FULLDUPLEX:
                    if (hostUser is null)
                        await hostGuild.AddBanAsync(clientBannedUser.Id, reason: reason);
                    else
                        await hostGuild.AddBanAsync(hostUser, reason: reason);
                    if (hostChannel != null)
                        await hostChannel.SendMessageAsync(embed: embedBan);
                    banCount++;
                    break;
                default:
                    DEFAULT:
                    declineCount++;
                    continue;
            }
        }
        
        if (clientChannel != null)
            await clientChannel.SendMessageAsync(embed: embedSync);
    }
}