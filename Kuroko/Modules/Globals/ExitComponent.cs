using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Services;

namespace Kuroko.Modules.Globals
{
    public class ExitComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{GlobalCommandMap.Exit}:*")]
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
            {
                msg.DeleteTimeout();
                await msg.DeleteAsync();
            }
        }
    }
}
