using Discord;
using Discord.Interactions;
using Kuroko.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class KurokoUserPermission(GuildPermission permission) : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(
        IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var config = services.GetRequiredService<KurokoConfig>();
        var user = context.User as IGuildUser;

        if (!(user!.GuildPermissions.Has(permission) ||
              user.GuildPermissions.Administrator ||
              config.AdminUserIds.Contains(user.Id) ||
              user.Id == config.OwnerId))
            return Task.FromResult(PreconditionResult.FromError(
                $"{Format.Bold("ACCESS DENIED:")} Missing {permission} Server Permission!"));

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}