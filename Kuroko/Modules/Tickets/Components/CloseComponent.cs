using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Database;

namespace Kuroko.Modules.Tickets.Components
{
    public class CloseComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{TicketsCommandMap.CloseTicket}:*")]
        public async Task ExecuteAsync(int ticketId)
        {
            // TODO: Close Ticket & present transcript + delete buttons
            var root = await Context.Database.Guilds.GetOrCreateRootAsync(Context.Guild.Id);
            var channel = Context.Channel as ITextChannel;
            var ticket = Context.Database.Tickets.FirstOrDefault(x => x.Id == ticketId);

            root.Tickets.Remove(ticket, Context.Database);

            await channel.DeleteAsync();
        }
    }
}
