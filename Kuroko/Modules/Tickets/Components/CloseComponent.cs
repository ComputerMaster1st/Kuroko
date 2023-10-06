using Discord;
using Discord.Interactions;
using Kuroko.Core;

namespace Kuroko.Modules.Tickets.Components
{
    public class CloseComponent : KurokoModuleBase
    {
        [ComponentInteraction(TicketsCommandMap.CloseTicket)]
        public async Task ExecuteAsync()
        {
            var channel = Context.Interaction.Channel as ITextChannel;
            await channel.DeleteAsync();
        }
    }
}
