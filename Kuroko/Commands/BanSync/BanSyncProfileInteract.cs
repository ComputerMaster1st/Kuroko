using Discord;
using Discord.Interactions;
using Kuroko.Attributes;
using Kuroko.Database.GuildEntities.Extras;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Commands.BanSync;

[KurokoBotPermission(GuildPermission.BanMembers)]
[KurokoUserPermission(GuildPermission.ManageGuild)]
public class BanSyncProfileInteract : KurokoCommandBase
{
    [SlashCommand("bansync-profile", "Show BanSync Profile for selected guild/server")]
    public async Task BanSyncProfileAsync([Autocomplete(typeof(BanSyncProfileAutocomplete))] int profileId)
    {
        var profile = await Context.Database.BanSyncProfiles
            .Include(banSyncProfile => banSyncProfile.HostProperties)
            .Include(banSyncProfile => banSyncProfile.ClientProperties).FirstOrDefaultAsync(
                x => x.Id == profileId);
        
        await ExecuteGuiAsync(profile);
    }

    [ComponentInteraction($"{CommandMap.BANSYNC_PROFILE_UPDATE}:*,*")]
    public async Task UpdateProfileAsync(ulong interactedUserId, int profileId, string rawMode)
    {
        if (!IsInteractedUser(interactedUserId)) return;
        await DeferAsync();
        
        var mode = Enum.Parse<BanSyncMode>(rawMode);
        var profile = await Context.Database.BanSyncProfiles
            .Include(banSyncProfile => banSyncProfile.HostProperties)
            .Include(banSyncProfile => banSyncProfile.ClientProperties).FirstOrDefaultAsync(
                x => x.Id == profileId);

        profile.Mode = mode;

        await ExecuteGuiAsync(profile, true);
    }

    [ComponentInteraction($"{CommandMap.BANSYNC_PROFILE_CANCEL}:*,*")]
    public async Task CancelProfileAsync(ulong interactedUserId, int profileId)
    {
        if (!IsInteractedUser(interactedUserId)) return;
        await DeferAsync();

        var profile = await Context.Database.BanSyncProfiles.FirstOrDefaultAsync(
            x => x.Id == profileId);
        if (profile is null)
            return;
        
        Context.Database.BanSyncProfiles.Remove(profile);
        
        var embed = new EmbedBuilder
        {
            Title = "BanSync Profile Cancelled!",
            Color = Color.Green,
            Timestamp = DateTimeOffset.Now,
            Description = "BanSync Profile has been cancelled!"
        }.Build();
        
        await Context.Channel.SendMessageAsync(embed: embed);
        
        var msg = await Context.Interaction.GetOriginalResponseAsync();
        if (msg != null)
            await msg.DeleteAsync();
    }
    
    private async Task ExecuteGuiAsync(BanSyncProfile profile, bool isReturning = false)
    {
        var isHost = false;
        string thumbnailUrl;
        string guildName;
        string bansyncId;
        if (profile.HostProperties.GuildId == Context.Guild.Id)
        {
            var guild = Context.Client.GetGuild(profile.ClientProperties.GuildId);
            thumbnailUrl = guild.IconUrl;
            guildName = guild.Name;
            bansyncId = profile.ClientSyncId.ToString();
            isHost = true;
        }
        else
        {
            var guild = Context.Client.GetGuild(profile.HostProperties.GuildId);
            thumbnailUrl = guild.IconUrl;
            guildName = guild.Name;
            bansyncId = profile.HostSyncId.ToString();
        }

        var componentBuilder = new ComponentBuilder();
        if (isHost)
            componentBuilder
                .WithSelectMenu($"{CommandMap.BANSYNC_PROFILE_UPDATE}:{Context.User.Id},{profile.Id}",
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
                    maxValues: 1);
        componentBuilder
            .WithButton("Cancel Sync", $"{
                CommandMap.BANSYNC_PROFILE_CANCEL}:{Context.User.Id},{profile.Id}",
                ButtonStyle.Danger)
            .WithButton("Exit", $"{CommandMap.EXIT_WITH_UID}:{Context.User.Id}",
                ButtonStyle.Secondary);
        
        var embedBuilder = new EmbedBuilder
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
                    Value = profile.Mode.ToString()
                }
            ]
        };
        
        if (!isReturning)
            await RespondAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());
        else
        {
            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = embedBuilder.Build();
                x.Components = componentBuilder.Build();
            });
        }
    }
}