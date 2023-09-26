using Discord;
using Discord.Interactions;

namespace Kuroko.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireBotGuildPermission : PreconditionAttribute
    {
        private readonly GuildPermission _permission;

        public RequireBotGuildPermission(GuildPermission permission)
            => _permission = permission;

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var user = await context.Guild.GetCurrentUserAsync();

            if (!(user.GuildPermissions.Has(_permission) ||
                user.GuildPermissions.Administrator))
            {
                return PreconditionResult.FromError(
                    string.Format("{0} Bot Missing {1} Server Permission!",
                        Format.Bold("ACCESS DENIED:"),
                        _permission));
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
