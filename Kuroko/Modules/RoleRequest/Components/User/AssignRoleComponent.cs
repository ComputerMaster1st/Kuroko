using Discord;
using Discord.Interactions;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.RoleRequest.Components.User
{
    public class AssignRoleComponent : RoleRequestBase
    {
        private static StringBuilder OutputMsg => new StringBuilder()
                    .AppendLine("# Role Request")
                    .AppendLine("## Assign Role");

        [ComponentInteraction($"{CommandIdMap.RoleRequestAssign}:*,*")]
        public async Task EntryAsync(ulong interactedUserId, int index)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();
            await ExecuteAsync(index, OutputMsg);
        }

        [ComponentInteraction($"{CommandIdMap.RoleRequestSave}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string[] roleIds)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();

            var selectedRoleIds = roleIds.Select(ulong.Parse);
            var output = OutputMsg.AppendLine("Assiged Roles:");
            var user = Context.User as IGuildUser;

            await user.AddRolesAsync(selectedRoleIds);

            foreach (var roleId in selectedRoleIds)
            {
                var role = user.Guild.GetRole(roleId);
                output.AppendLine("* " + role.Name);
            }

            await ExecuteAsync(index, output);
        }

        private async Task ExecuteAsync(int index, StringBuilder output)
        {
            var menu = RRMenu.BuildAssignMenu(Context.User as IGuildUser, await GetProperties(), index);

            if (!menu.HasOptions)
                output.AppendLine()
                    .AppendLine("Oops, looks like you reached the end of the list already!");

            (await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            })).ResetTimeout();
        }
    }
}
