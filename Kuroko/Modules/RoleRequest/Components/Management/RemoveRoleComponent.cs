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
    public class RemoveRoleComponent : KurokoModuleBase
    {
        private static StringBuilder OutputMsg
        {
            get
            {
                return new StringBuilder()
                    .AppendLine("# Role Request")
                    .AppendLine("## Management")
                    .AppendLine("### Remove Roles");
            }
        }

        [ComponentInteraction($"{CommandIdMap.RoleRequestManageRemove}:*,*")]
        public async Task InitialAsync(ulong interactedUserId, int index)
        {
            await DeferAsync();

            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            var roleRequest = await Context.Database.GuildRoleRequests.CreateOrGetDataAsync(
                Context.Database.Guilds, Context.Guild.Id, (x, y) =>
                {
                    x.RoleRequest ??= y;
                });

            await ExecuteAsync(roleRequest, index, OutputMsg);
        }

        [ComponentInteraction($"{CommandIdMap.RoleRequestManageDelete}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string[] roleIds)
        {
            await DeferAsync();

            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            var roleRequest = await Context.Database.GuildRoleRequests.CreateOrGetDataAsync(
                Context.Database.Guilds, Context.Guild.Id, (x, y) =>
                {
                    x.RoleRequest ??= y;
                });
            var selectedRoleIds = roleIds.Select(ulong.Parse);

            OutputMsg.AppendLine("Selected roles removed from public use:");

            foreach (var roleId in selectedRoleIds)
            {
                var role = Context.Guild.GetRole(roleId);
                roleRequest.RoleIds.RemoveAll(x => x.Value == roleId);

                OutputMsg.AppendLine("* " + role.Name);
            }

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(roleRequest, index, OutputMsg);
        }

        private async Task ExecuteAsync(RoleRequestEntity roleRequest, int index, StringBuilder output)
        {
            var menu = RRMenu.BuildRemoveMenu(Context.User as IGuildUser, roleRequest, index);

            if (!menu.HasOptions)
                output.AppendLine("Nothing to list.");

            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            });
        }
    }
}
