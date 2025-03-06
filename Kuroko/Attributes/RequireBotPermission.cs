using Discord;
using Discord.Interactions;

namespace Kuroko.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireBotPermission(GuildPermission permission) : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(
        IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var user = await context.Guild.GetCurrentUserAsync();

        if (!(user.GuildPermissions.Has(permission) ||
              user.GuildPermissions.Administrator))
            return PreconditionResult.FromError(
                $"{Format.Bold("ACCESS DENIED:")} Bot Missing {permission} Server Permission!");
            
        return PreconditionResult.FromSuccess();
    }
}