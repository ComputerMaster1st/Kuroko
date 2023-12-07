using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TicketPreconditionAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var ctx = context as KurokoInteractionContext;
            var properties = await ctx.Database.GuildReports.FirstOrDefaultAsync(x => x.GuildId == ctx.Guild.Id);

            if (properties is null || properties.ReportCategoryId == 0)
                return PreconditionResult.FromError(
                    $"{Format.Bold("TICKETS DISABLED:")} Server not configured for tickets. Please contact server management!");
            
            if (properties.ReportHandlers.Count < 1)
                return PreconditionResult.FromError(
                    $"{Format.Bold("TICKETS DISABLED:")} No handlers configured for tickets. Please contact server management!");
            
            return PreconditionResult.FromSuccess();
        }
    }
}