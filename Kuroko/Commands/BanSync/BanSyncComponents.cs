using Discord;
using Discord.Interactions;
using Kuroko.Attributes;

namespace Kuroko.Commands.BanSync;

[KurokoBotPermission(GuildPermission.BanMembers)]
[KurokoUserPermission(GuildPermission.BanMembers)]
public class BanSyncComponents : KurokoCommandBase
{
    [ComponentInteraction($"{CommandMap.BANSYNC_BANUSER}:*,*")]
    public Task BanUserAsync(ulong bannedUserId, int verifyCount)
        => ConfirmBanUserAsync(Context.User.Id, bannedUserId, verifyCount);

    [ComponentInteraction($"{CommandMap.BANSYNC_BANUSER}:*,*,*")]
    public async Task ConfirmBanUserAsync(ulong interactedUserId, ulong bannedUserId, int verifyCount)
    {
        await DeferAsync();
        
        if (!IsInteractedUser(interactedUserId)) return;
        verifyCount--;

        if (verifyCount < 1)
        {
            var bannedUser = Context.Guild.GetUser(bannedUserId);
            var msg = await Context.Interaction.GetOriginalResponseAsync();
            var embedBuilder = msg.Embeds.First().ToEmbedBuilder();
            embedBuilder.WithTitle("User Banned!");
            var reason = embedBuilder.Fields.First(f => f.Name == "Reason").Value as string;

            if (bannedUser != null)
                await Context.Guild.AddBanAsync(bannedUser, reason: reason);
            else
                await Context.Guild.AddBanAsync(bannedUserId, reason: reason);

            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = embedBuilder.Build();
                x.Components = null;
            });
            return;
        }

        var components = new ComponentBuilder()
            .WithButton($"({verifyCount}) !! Ban User !! ({verifyCount})", $"{
                CommandMap.BANSYNC_BANUSER}:{interactedUserId},{bannedUserId},{verifyCount}",
                ButtonStyle.Danger)
            .WithButton("Ignore", $"{CommandMap.BANSYNC_IGNORE}", ButtonStyle.Secondary)
            .Build();
        
        await Context.Interaction.ModifyOriginalResponseAsync(x =>
        {
            x.Components = components;
        });
    }
}