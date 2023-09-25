using Discord.Interactions;
using Kuroko.Core;

namespace Kuroko.Modules.Globals
{
    // TODO: This will handle exiting of main menu

    public class ExitComponent : KurokoModuleBase
    {
        [ComponentInteraction(CommandIdMap.Exit)]
        public async Task ExecuteAsync()
        {
            await DeferAsync();

            var msg = await Context.Interaction.GetOriginalResponseAsync();
            await msg.DeleteAsync();
        }
    }
}
