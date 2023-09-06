using Discord;
using Discord.Interactions;
using Kuroko.Core.Configuration;
using Kuroko.Database;
using Kuroko.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class OptionalUserGuildPermission<TRequirePermission> : PreconditionAttribute
        where TRequirePermission : class, IRequirePermission
    {
        private readonly GuildPermission _permission;

        public OptionalUserGuildPermission(GuildPermission permission)
            => _permission = permission;

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var config = services.GetRequiredService<KDiscordConfig>();
            var db = services.GetRequiredService<DatabaseContext>();
            var set = db.Set<TRequirePermission>();
            var profile = await set.FirstOrDefaultAsync(x => x.Guild.Id == context.Guild.Id);
            var user = context.User as IGuildUser;

            if (profile != null && !profile.IsPermissionRequired)
                return PreconditionResult.FromSuccess();

            if (!(user.GuildPermissions.Has(_permission) ||
                user.GuildPermissions.Administrator ||
                config.BotAdminUserIds.Contains(user.Id) ||
                user.Id == config.BotOwnerId))
            {
                return PreconditionResult.FromError(
                    string.Format("{0} Missing {1} Server Permission!",
                        Format.Bold("ACCESS DENIED:"),
                        _permission));
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
