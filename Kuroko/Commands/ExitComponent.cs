using Discord;
using Discord.Interactions;
using Kuroko.Attributes;

namespace Kuroko.Commands;

public class ExitComponent : KurokoCommandBase
{
    [ComponentInteraction($"{CommandMap.EXIT_WITH_UID}:*")]
    public async Task ExecuteAsync(ulong interactedUserId)
    {
        if (interactedUserId != Context.User.Id)
            return;
        await ExecuteAsync();
    }

    [ComponentInteraction($"{CommandMap.EXIT_WITH_PERM}")]
    [KurokoUserPermission(GuildPermission.ManageGuild)]
    public async Task ExecuteAsync()
    {
        await DeferAsync();
        
        var msg = await Context.Interaction.GetOriginalResponseAsync();
        if (msg != null)
            await msg.DeleteAsync();
    }
}