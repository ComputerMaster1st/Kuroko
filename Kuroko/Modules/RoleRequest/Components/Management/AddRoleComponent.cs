using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using System.Text;

namespace Kuroko.Modules.RoleRequest.Components.Management
{
    [RequireUserGuildPermission(GuildPermission.ManageRoles)]
    [RequireBotGuildPermission(GuildPermission.ManageRoles)]
    public class AddRoleComponent : KurokoModuleBase
    {
        private static StringBuilder OutputMsg
        {
            get
            {
                return new StringBuilder()
                    .AppendLine("# Role Request")
                    .AppendLine("## Management")
                    .AppendLine("### Add Roles");
            }
        }

        [ComponentInteraction($"{CommandIdMap.RoleRequestManageAdd}:*,*")]
        public async Task InitialAsync(ulong interactedUserId, int index)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();

            var roleRequest = await Context.Database.GuildRoleRequests.CreateOrGetDataAsync(
                Context.Database.Guilds, Context.Guild.Id, (x, y) =>
                {
                    x.RoleRequest ??= y;
                });

            await ExecuteAsync(roleRequest, index, OutputMsg);
        }

        [ComponentInteraction($"{CommandIdMap.RoleRequestManageSave}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string[] roleIds)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();

            var properties = await Context.Database.GuildRoleRequests.CreateOrGetDataAsync(
                Context.Database.Guilds, Context.Guild.Id, (x, y) =>
                {
                    x.RoleRequest ??= y;
                });
            var selectedRoleIds = roleIds.Select(ulong.Parse);

            var output = OutputMsg.AppendLine("Selected roles for public use:");

            foreach (var roleId in selectedRoleIds)
            {
                var role = Context.Guild.GetRole(roleId);

                if (properties.RoleIds.Any(x => x.Value == role.Id))
                    output.AppendLine("* **Already Available** - " + role.Name);
                else
                {
                    properties.RoleIds.Add(new(role.Id));
                    output.AppendLine("* " + role.Name);
                }
            }

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(properties, index, output);
        }

        private async Task ExecuteAsync(RoleRequestEntity roleRequest, int index, StringBuilder output)
        {
            var menu = RRMenu.BuildAddMenu(Context.User as IGuildUser, roleRequest, index);

            if (!menu.HasOptions)
                output.AppendLine("All roles already available! Nothing to list.");

            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            });
        }
    }
}
