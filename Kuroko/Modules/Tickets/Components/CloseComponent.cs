using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Database.Entities.Guild;
using Kuroko.Services;

namespace Kuroko.Modules.Tickets.Components
{
    public class CloseComponent : KurokoModuleBase
    {
        private readonly TicketService _tickets;

        public CloseComponent(TicketService tickets)
            => _tickets = tickets;

        [ComponentInteraction($"{TicketsCommandMap.CLOSE}:*")]
        public async Task ExecuteAsync(int ticketId)
        {
            var properties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            ITextChannel chn = null;

            if (properties.TranscriptsChannelId != 0)
            {
                chn = Context.Guild.GetTextChannel(properties.TranscriptsChannelId);

                if (chn is null)
                {
                    await (Context.Channel as ITextChannel).DeleteAsync();
                    return;
                }
            }

            await RespondAsync("Ticket Closed! Performing cleanup now...");
            await Task.Delay(2000);
            await _tickets.BuildAndSendTranscriptAsync(properties, Context.Guild, Context.Channel as ITextChannel, chn, ticketId);
        }
    }
}
