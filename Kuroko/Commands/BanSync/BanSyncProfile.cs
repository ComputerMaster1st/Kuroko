using Discord;
using Discord.Interactions;
using Kuroko.Attributes;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Commands.BanSync;

[KurokoBotPermission(GuildPermission.BanMembers)]
[KurokoUserPermission(GuildPermission.ManageGuild)]
public class BanSyncProfileInteract : KurokoCommandBase
{
    [SlashCommand("bansync-profile", "Show BanSync Profile for selected guild/server")]
    public Task BanSyncProfileAsync([Autocomplete(typeof(BanSyncProfileAutocomplete))] int profileId)
        => ExecuteGuiAsync(profileId);
    
    private async Task ExecuteGuiAsync(int profileId, bool isReturning = false)
    {
        var bansyncProfile = await Context.Database.BanSyncProfiles
            .Include(banSyncProfile => banSyncProfile.HostProperties)
            .Include(banSyncProfile => banSyncProfile.ClientProperties).FirstOrDefaultAsync(
            x => x.Id == profileId);
        
        var isHost = false;
        string thumbnailUrl;
        string guildName;
        string bansyncId;
        if (bansyncProfile.HostProperties.GuildId == Context.Guild.Id)
        {
            var guild = Context.Client.GetGuild(bansyncProfile.ClientProperties.GuildId);
            thumbnailUrl = guild.IconUrl;
            guildName = guild.Name;
            bansyncId = bansyncProfile.ClientSyncId.ToString();
            isHost = true;
        }
        else
        {
            var guild = Context.Client.GetGuild(bansyncProfile.HostProperties.GuildId);
            thumbnailUrl = guild.IconUrl;
            guildName = guild.Name;
            bansyncId = bansyncProfile.HostSyncId.ToString();
        }

        MessageComponent components = null;
        if (isHost)
            components = new ComponentBuilder()
                .WithSelectMenu($"{CommandMap.BANSYNC_PROFILE_UPDATE}:{Context.User.Id},{bansyncProfile.Id}", 
                    [
                        new SelectMenuOptionBuilder
                        {
                            Label = nameof(BanSyncMode.Default),
                            Value = nameof(BanSyncMode.Default)
                        },
                        new SelectMenuOptionBuilder
                        {
                            Label = nameof(BanSyncMode.Simplex),
                            Value = nameof(BanSyncMode.Simplex)
                        },
                        new SelectMenuOptionBuilder
                        {
                            Label = nameof(BanSyncMode.HalfDuplex),
                            Value = nameof(BanSyncMode.HalfDuplex)
                        },
                        new SelectMenuOptionBuilder
                        {
                            Label = nameof(BanSyncMode.FullDuplex),
                            Value = nameof(BanSyncMode.FullDuplex)
                        }
                    ],
                    $"Update BanSync Mode...",
                    maxValues: 1)
                .WithButton("Cancel Sync", $"{CommandMap.BANSYNC_PROFILE_CANCEL}:{Context.User.Id}",
                    ButtonStyle.Danger)
                .WithButton("Exit", $"{CommandMap.EXIT_WITH_UID}:{Context.User.Id}",
                    ButtonStyle.Secondary)
                .Build();
        
        var embed = new EmbedBuilder
        {
            Title = "BanSync Profile Information",
            Color = Color.Blue,
            Timestamp = DateTimeOffset.Now,
            ThumbnailUrl = thumbnailUrl,
            Fields = [
                new EmbedFieldBuilder
                {
                    Name = "BanSync Id",
                    Value = bansyncId
                },
                new EmbedFieldBuilder
                {
                    Name = $"{(isHost ? "Client" : "Host")} Server/Guild",
                    Value = guildName
                },
                new EmbedFieldBuilder
                {
                    Name = "BanSync Mode",
                    Value = bansyncProfile.Mode.ToString()
                }
            ]
        }.Build();
        
        if (!isReturning)
            await RespondAsync(embed: embed, components: components);
        else
            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = embed;
                x.Components = components;
            });
    }
}