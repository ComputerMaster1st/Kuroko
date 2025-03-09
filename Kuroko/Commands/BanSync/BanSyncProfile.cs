using Discord;
using Discord.Interactions;
using Kuroko.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Commands.BanSync;

[KurokoBotPermission(GuildPermission.BanMembers)]
[KurokoUserPermission(GuildPermission.ManageGuild)]
public class BanSyncProfileInteract : KurokoCommandBase
{
    [SlashCommand("bansync-profile", "Show BanSync Profile for selected guild/server")]
    public async Task BanSyncProfileAsync([Autocomplete(typeof(BanSyncProfileAutocomplete))] int profileId)
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
        
        var embed = new EmbedBuilder()
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
        
        await RespondAsync(embed: embed, ephemeral: true);
    }
}