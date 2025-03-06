using Discord.Interactions;

namespace Kuroko.Commands;

public class ExitComponent : KurokoCommandBase
{
    [ComponentInteraction($"{CommandMap.EXIT}:*")]
    public async Task ExecuteAsync(ulong interactedUserId)
    {
        if (interactedUserId != Context.User.Id)
            return;
        await DeferAsync();

        var msg = await Context.Interaction.GetOriginalResponseAsync();
        if (msg != null)
            await msg.DeleteAsync();
    }
}