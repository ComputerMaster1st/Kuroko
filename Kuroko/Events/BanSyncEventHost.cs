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
public class BanSyncEventHost : BanSyncEventBase
{
    private readonly DiscordShardedClient _client;
    private readonly IServiceProvider _services;
    
    public BanSyncEventHost(DiscordShardedClient client, IServiceProvider services)
    {
        _client = client;
        _services = services;
        
        client.UserBanned += (user, guild) 
            => Task.Factory.StartNew(() => ClientOnUserBannedAsync(user, guild));
    }

    private async Task ClientOnUserBannedAsync(SocketUser hostBannedUser, SocketGuild hostGuild)
    {
        await using var database = _services.GetRequiredService<DatabaseContext>();
        var properties = await database.BanSyncProperties
            .Include(banSyncProperties => banSyncProperties.HostForProfiles)
            .FirstOrDefaultAsync(x => x.RootId == hostGuild.Id);
        
        if (!properties.IsEnabled)
            return;
        
        var components = new ComponentBuilder()
            .WithButton("(3) !! Ban User !! (3)", $"{CommandMap.BANSYNC_BANUSER}:{hostBannedUser.Id},3",
                ButtonStyle.Danger)
            .WithButton("Ignore", $"{CommandMap.BANSYNC_IGNORE}", ButtonStyle.Secondary)
            .Build();
        var hostChannel = hostGuild.GetTextChannel(properties.BanSyncChannelId);
        var banReason = await hostGuild.GetBanAsync(hostBannedUser.Id);
        var reason = $"[BanSync] {
            (string.IsNullOrEmpty(banReason.Reason) ? "No Reason Given" : $"Reason: {banReason.Reason}")}";
        var embedWarn = CreateEmbed("BanSync - Incoming Warning!", hostBannedUser.GlobalName,
            hostBannedUser.Id, $"User Banned From: {hostGuild.Name}",
            hostBannedUser.GetAvatarUrl(), reason);
        var embedBan = CreateEmbed("BanSync - Auto-Ban Performed!", hostBannedUser.GlobalName,
            hostBannedUser.Id, $"User Banned From: {hostGuild.Name}",
            hostBannedUser.GetAvatarUrl(), reason);
        var declineCount = 0;
        var warnCount = 0;
        var banCount = 0;
        
        foreach (var profile in properties.HostForProfiles)
        {
            if (!profile.ClientGuildProperties.IsEnabled)
                return;
            
            var clientGuild = _client.GetGuild(profile.ClientGuildProperties.RootId);
            var clientUser = clientGuild.GetUser(hostBannedUser.Id);
            var clientChannel = clientGuild.GetTextChannel(profile.ClientGuildProperties.BanSyncChannelId);

            if (await clientGuild.GetBanAsync(hostBannedUser.Id) != null)
                continue;

            switch (profile.ClientGuildProperties.ClientMode)
            {
                case BanSyncMode.HalfDuplex:
                    if (clientChannel != null)
                        await clientChannel.SendMessageAsync(embed: embedWarn, components: components);
                    warnCount++;
                    break;
                case BanSyncMode.FullDuplex:
                    if (clientUser is null)
                        await clientGuild.AddBanAsync(hostBannedUser.Id, reason: reason);
                    else
                        await clientGuild.AddBanAsync(clientUser, reason: reason);
                    if (clientChannel != null)
                        await clientChannel.SendMessageAsync(embed: embedBan);
                    banCount++;
                    break;
                default:
                    declineCount++;
                    continue;
            }
        }
        
        var embedSync = CreateEmbed("BanSync - User Banned & Synced!", 
            hostBannedUser.GlobalName, hostBannedUser.Id, 
            new StringBuilder()
                .AppendLine($"Clients Declined: {declineCount}")
                .AppendLine($"Warnings Sent: {warnCount}")
                .AppendLine($"Bans Executed: {banCount}")
                .ToString(),
            hostBannedUser.GetAvatarUrl(), reason);
        
        if (hostChannel != null)
            await hostChannel.SendMessageAsync(embed: embedSync);
    }
}