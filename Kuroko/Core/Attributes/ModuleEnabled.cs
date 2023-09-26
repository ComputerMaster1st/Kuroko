using Discord;
using Discord.Interactions;
using Kuroko.Database;
using Kuroko.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Core.Attributes
{
    public class IsModuleEnabled<TModuleEnabled> : PreconditionAttribute where TModuleEnabled : class, IModuleEnabled
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var db = services.GetRequiredService<DatabaseContext>();
            var set = db.Set<TModuleEnabled>();
            var profile = await set.FirstOrDefaultAsync(x => x.Guild.Id == context.Guild.Id);

            if (profile.IsEnabled)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError(string.Format("{0} {1}",
                Format.Bold("COMMAND DISABLED:"),
                string.Format("The command {0} is currently disabled. Contact server moderator for more info.",
                    Format.Bold(commandInfo.Name))));
        }
    }
}
