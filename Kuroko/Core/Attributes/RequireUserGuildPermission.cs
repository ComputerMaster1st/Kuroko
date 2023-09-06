using Discord;
using Discord.Interactions;
using Kuroko.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireUserGuildPermission : PreconditionAttribute
    {
        private readonly GuildPermission _permission;

        public RequireUserGuildPermission(GuildPermission permission)
            => _permission = permission;

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var config = services.GetRequiredService<KDiscordConfig>();
            var user = context.User as IGuildUser;

            if (!(user.GuildPermissions.Has(_permission) ||
                user.GuildPermissions.Administrator ||
                config.BotAdminUserIds.Contains(user.Id) ||
                user.Id == config.BotOwnerId))
            {
                return Task.FromResult(PreconditionResult.FromError(
                    string.Format("{0} Missing {1} Server Permission!",
                        Format.Bold("ACCESS DENIED:"),
                        _permission)));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
