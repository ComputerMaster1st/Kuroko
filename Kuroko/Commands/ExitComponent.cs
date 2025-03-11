using Discord;
using Discord.Interactions;
using Kuroko.Attributes;

namespace Kuroko.Commands;

public class ExitComponent : KurokoCommandBase
{
    [ComponentInteraction($"{CommandMap.EXIT_WITH_UID}:*")]
    public Task ExitWithUidAsync(ulong interactedUserId)
        => !IsInteractedUser(interactedUserId) ? Task.CompletedTask : ExitAsync();


    [ComponentInteraction($"{CommandMap.EXIT_WITH_PERM}")]
    [KurokoUserPermission(GuildPermission.ManageGuild)]
    public Task ExitWithManageGuildPermAsync()
        => ExitAsync();

    [ComponentInteraction($"{CommandMap.BANSYNC_IGNORE}")]
    [KurokoUserPermission(GuildPermission.BanMembers)]
    public Task ExitWithBanPermAsync()
        => ExitAsync();
    
    private async Task ExitAsync()
    {
        await DeferAsync();
        
        var msg = await Context.Interaction.GetOriginalResponseAsync();
        if (msg != null)
            await msg.DeleteAsync();
    }
    
}