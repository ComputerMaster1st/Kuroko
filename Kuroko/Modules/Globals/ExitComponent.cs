using Discord.Interactions;
using Kuroko.Core;

namespace Kuroko.Modules.Globals
{
    // TODO: This will handle exiting of main menu

    public class ExitComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{CommandIdMap.Exit}:*")]
        public async Task ExecuteAsync(ulong interactedUserId)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();

            var msg = await Context.Interaction.GetOriginalResponseAsync();

            if (msg != null)
                await msg.DeleteAsync();
        }
    }
}
