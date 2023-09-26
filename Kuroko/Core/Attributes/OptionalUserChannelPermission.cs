using Discord;
using Discord.Interactions;
using Kuroko.Database;
using Kuroko.Database.Entities;
using Kuroko.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class OptionalUserChannelPermission<TRequirePermission> : PreconditionAttribute
        where TRequirePermission : class, IRequirePermission
    {
        private readonly bool _overridePermCheck;
        private readonly ChannelPermission _permission;

        public OptionalUserChannelPermission(ChannelPermission permission, bool overridePermCheck = false)
        {
            _overridePermCheck = overridePermCheck;
            _permission = permission;
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var config = services.GetRequiredService<KDiscordConfig>();
            var db = services.GetRequiredService<DatabaseContext>();
            var set = db.Set<TRequirePermission>();
            var profile = await set.FirstOrDefaultAsync(x => x.Guild.Id == context.Guild.Id);
            var user = context.User as IGuildUser;

            if (profile != null && !profile.IsPermissionRequired && !_overridePermCheck)
                return PreconditionResult.FromSuccess();

            var channelPerms = user.GetPermissions(context.Channel as IGuildChannel);

            if (!(channelPerms.Has(_permission) ||
                user.GuildPermissions.Administrator ||
                config.BotAdminUserIds.Contains(user.Id) ||
                user.Id == config.BotOwnerId))
            {
                return PreconditionResult.FromError(
                    string.Format("{0} Missing {1} Channel Permission!",
                        Format.Bold("ACCESS DENIED:"),
                        _permission));
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
