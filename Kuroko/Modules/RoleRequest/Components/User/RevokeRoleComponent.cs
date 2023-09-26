using Discord;
using Discord.Interactions;
using System.Text;

namespace Kuroko.Modules.RoleRequest.Components.User
{
    public class RevokeAssignRoleComponent : RoleRequestBase
    {
        private static StringBuilder OutputMsg => new StringBuilder()
                    .AppendLine("# Role Request")
                    .AppendLine("## Remove Role");

        [ComponentInteraction($"{CommandIdMap.RoleRequestRemove}:*,*")]
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

        [ComponentInteraction($"{CommandIdMap.RoleRequestDelete}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string[] roleIds)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();

            var selectedRoleIds = roleIds.Select(ulong.Parse);
            var output = OutputMsg.AppendLine("Removed Roles:");
            var user = Context.User as IGuildUser;

            await user.RemoveRolesAsync(selectedRoleIds);

            foreach (var roleId in selectedRoleIds)
            {
                var role = user.Guild.GetRole(roleId);
                output.AppendLine("* " + role.Name);
            }

            await ExecuteAsync(index, output);
        }

        private async Task ExecuteAsync(int index, StringBuilder output)
        {
            var self = Context.Guild.GetUser(Context.Client.CurrentUser.Id);
            var menu = RRMenu.BuildRevokeMenu(self, Context.User as IGuildUser, await GetProperties(), index);

            if (!menu.HasOptions)
                output.AppendLine()
                    .AppendLine("Oops, looks like you reached the end of the list already!");

            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            });
        }
    }
}
