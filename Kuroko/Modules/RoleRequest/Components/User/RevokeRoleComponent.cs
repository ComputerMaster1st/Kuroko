using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.RoleRequest.Components.User
{
    public class RevokeAssignRoleComponent : KurokoModuleBase
    {
        private static StringBuilder OutputMsg => new StringBuilder()
                    .AppendLine("# Role Request")
                    .AppendLine("## Remove Role");

        [ComponentInteraction($"{RoleRequestCommandMap.RoleRequestRemove}:*,*")]
        public async Task EntryAsync(ulong interactedUserId, int index)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();
            await ExecuteAsync(index, OutputMsg);
        }

        [ComponentInteraction($"{RoleRequestCommandMap.RoleRequestDelete}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string[] roleIds)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

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
            var self = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var user = Context.User as IGuildUser;
            var count = 0;
            var roleIds = user.RoleIds.Skip(index).ToList();
            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"{RoleRequestCommandMap.RoleRequestDelete}:{user.Id},{index}")
                .WithMinValues(1)
                .WithPlaceholder("Select role(s) to remove from yourself");
            var guildRoles = new List<IRole>();
            IRole selfHighestRole = null;

            foreach (var roleId in self.RoleIds)
            {
                var role = self.Guild.GetRole(roleId);

                if (selfHighestRole is null || role.Position > selfHighestRole.Position)
                    selfHighestRole = role;
            }

            foreach (var roleId in roleIds)
            {
                var role = user.Guild.GetRole(roleId);
                guildRoles.Add(role);
            }

            foreach (var role in guildRoles.OrderByDescending(x => x.Position))
            {
                if (role.Position >= selfHighestRole.Position || role.Name == "@everyone")
                    continue;

                selectMenu.AddOption(role.Name, role.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            var menu = Pagination.SelectMenu(selectMenu, index, user, RoleRequestCommandMap.RoleRequestRemove, RoleRequestCommandMap.RoleRequestMenu);

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
