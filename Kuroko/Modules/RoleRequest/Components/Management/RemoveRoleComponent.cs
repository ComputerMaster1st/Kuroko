using Discord;
using Discord.Interactions;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.RoleRequest.Components.Management
{
    [RequireUserGuildPermission(GuildPermission.ManageRoles)]
    [RequireBotGuildPermission(GuildPermission.ManageRoles)]
    public class RemoveRoleComponent : RoleRequestBase
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
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();
            await ExecuteAsync(await GetProperties(), index, OutputMsg);
        }

        [ComponentInteraction($"{CommandIdMap.RoleRequestManageDelete}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string[] roleIds)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();

            var properties = await GetProperties();
            var selectedRoleIds = roleIds.Select(ulong.Parse);

            OutputMsg.AppendLine("Selected roles removed from public use:");

            foreach (var roleId in selectedRoleIds)
            {
                var role = Context.Guild.GetRole(roleId);
                properties.RoleIds.RemoveAll(x => x.Value == roleId);

                OutputMsg.AppendLine("* " + role.Name);
            }

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(properties, index, OutputMsg);
        }

        private async Task ExecuteAsync(RoleRequestEntity properties, int index, StringBuilder output)
        {
            var menu = RRMenu.BuildRemoveMenu(Context.User as IGuildUser, properties, index);

            if (!menu.HasOptions)
                output.AppendLine("Nothing to list.");

            (await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            })).ResetTimeout();
        }
    }
}
