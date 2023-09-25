using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;

namespace Kuroko.Modules.RoleRequest.Components.Management
{
    [RequireUserGuildPermission(GuildPermission.ManageRoles)]
    [RequireBotGuildPermission(GuildPermission.ManageRoles)]
    public class AddRoleComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{CommandIdMap.RoleRequestManageAdd}:*,*")]
        public async Task ExecuteAsync(ulong interactedUserId, int startIndex)
        {
            await DeferAsync();

            var roleRequest = await Context.Database.GuildRoleRequests.CreateOrGetDataAsync(
                Context.Database.Guilds, Context.Guild.Id, (x, y) =>
                {
                    x.RoleRequest ??= y;
                });

            // TODO: Make a menu for adding roles
        }
    }
}
